---
layout: post
title: OrmLite Blobbing done with NHibernate and Serialized JSON...
category: NHibernate
---

There seems to be a growing trend now with these Micro ORM's, at least that is what I see with [ServiceStack.OrmLite](https://github.com/ServiceStack/ServiceStack.OrmLite), which is the ability to persist properties of an object as a JSON, rather than in separate tables.

Usually with a Relational approach you would create a `Customer` table, `Address` table, and most likely shove the Phone Numbers under separate columns of the customer for `HomePhone` and `Mobile`.

This means we are limited to two types of phone numbers, and require joining or querying for the addresses.

Do we really need separate columns for phone numbers? Do we really need to persist the addresses in another table?

One problem I see with putting addresses into it's own table, is the temptation to relate them to an Order (assuming this is some sort of eCommerce system) when really there is no relationship between a customer's address, and the address on an order.

Really, the order should have it's own address, otherwise you can never delete or update an address on a customer, and you can't delete the customer. However I digress and this is a topic for another day.

<!--excerpt-->

## Example by OrmLite ##

This post is about how to do it with NHibernate, but I'm going to start by showing the example in OrmLite, then use the same example for NHibernate.

<span class="note">**Note:** OrmLite by default persists as JSV-format (JSON+CSV) rather than JSON. I'm currently unaware of any way to change it to be JSON.</span>

The customer is the root aggregate, and has his own Addresses and Phone Numbers, and is modelled like so:

    public enum PhoneType
    {
        Home,
        Work,
        Mobile,
    }
    
    public enum AddressType
    {
        Home,
        Work,
        Other,
    }

    public class Address
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class Customer
    {
        public Customer()
        {
            this.PhoneNumbers = new Dictionary<PhoneType, string>();
            this.Addresses = new Dictionary<AddressType, Address>();
        }

        [AutoIncrement] // Creates Auto primary key
        public virtual int Id { get; set; }
                   
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
                   
        [Index(Unique = true)] // Creates Unique Index
        public virtual string Email { get; set; }
                   
        public virtual Dictionary<PhoneType, string> PhoneNumbers { get; set; }  //Blobbed
        public virtual Dictionary<AddressType, Address> Addresses { get; set; }  //Blobbed
        public virtual DateTime CreatedAt { get; set; }
    }

<span class="note">**Note:** The attributes are for OrmLite and are not used by NHibernate, and the properties have been made virtual for NHibernate.</span>

So using OrmLite if we insert some data like so:

    var customer = new Customer
    {
        FirstName = "Phillip",
        LastName = "Haydon",
        Email = "test@test.com"
    };
    customer.Addresses.Add(AddressType.Home, new Address
    {
        Line1 = "Unit 31",
        Line2 = "102 Banana Street",
        City = "Sydney",
        Country = "Australia",
        State = "NSW",
        ZipCode = "2009"
    });

    customer.PhoneNumbers.Add(PhoneType.Mobile, "+61 411 122 34");
    customer.PhoneNumbers.Add(PhoneType.Home, "+61 256 3234");

    cmd.Insert(customer);

We can query for that data and we get the following results:

[![](/images/nhibernate-blobbing-1.png)](/images/nhibernate-blobbing-1.png)

*(Click on the image to see it fully)*

As you can see PhoneNumbers are stored like so:

> {Mobile:+61 411 122 34,Home:+61 256 3234}

And Addresses are stored like:

> {Home:{Line1:Unit 31,Line2:102 Banana Street,ZipCode:2009,State:NSW,City:Sydney,Country:Australia}}

Now if we query for that data back out:

    var customer = cmd.QuerySingle<Customer>(1);

![](/images/nhibernate-blobbing-2.png)

You can see we get all the information back out again, no problem! This stuff is built into OrmLite which is awesome, but how do we do it in NHibernate?

## Custom NHibernate UserType ##

So now we want to do this in NHibernate. This is a `UserType` I wrote a long time ago, well... I re-wrote it recently but wrote the initial idea a long time ago, and I've personally never seen anything similar in NHibernate.

I've put this on Gist - <https://gist.github.com/1936188>

    [Serializable]
    public class Blobbed<T> : IUserType where T : class
    {
        public new bool Equals(object x, object y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            var xdocX = JsonConvert.SerializeObject(x);
            var xdocY = JsonConvert.SerializeObject(y);

            return xdocY == xdocX;
        }

        public int GetHashCode(object x)
        {
            if (x == null)
                return 0;

            return x.GetHashCode();
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            if (names.Length != 1)
                throw new InvalidOperationException("Only expecting one column...");

            var val = rs[names[0]] as string;

            if (val != null && !string.IsNullOrWhiteSpace(val))
            {
                return JsonConvert.DeserializeObject<T>(val);
            }

            return null;
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            var parameter = (DbParameter)cmd.Parameters[index];

            if (value == null)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = JsonConvert.SerializeObject(value);
            }
        }

        public object DeepCopy(object value)
        {
            if (value == null)
                return null;

            //Serialized and Deserialized using json.net so that I don't
            //have to mark the class as serializable. Most likely slower
            //but only done for convenience.

            var serialized = JsonConvert.SerializeObject(value);

            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            var str = cached as string;

            if (string.IsNullOrWhiteSpace(str))
                return null;

            return JsonConvert.DeserializeObject<T>(str);
        }

        public object Disassemble(object value)
        {
            if (value == null)
                return null;

            return JsonConvert.SerializeObject(value);
        }

        public SqlType[] SqlTypes
        {
            get
            {
                return new SqlType[] { new StringSqlType() };
            }
        }

        public Type ReturnedType
        {
            get { return typeof(T); }
        }

        public bool IsMutable
        {
            get { return true; }
        }
    }

It's a generic class so that I can return the type of object back, and uses json.net to handle the serialization/deserialization of the object to JSON.

Now when mapping the properties we can specify the `CustomType` like so:

    Map(x => x.Addresses, "Addresses").CustomType<Blobbed<Dictionary<AddressType, Address>>>();
    Map(x => x.PhoneNumbers, "PhoneNumbers").CustomType<Blobbed<Dictionary<PhoneType, string>>>();
    
The example I'm using has two dictionaries of values. But if you were mapping a single type such as a single `Address`, you would just specify the type as above, without the `Dictionary`, `.CustomType<Blobbed<Address>>()`

The full mapping for the Customer is:

    public class CustomerMap : ClassMap<Customer>
    {
        public CustomerMap()
        {
            Table("Customer");
            Id(x => x.Id, "Id").GeneratedBy.Identity();

            Map(x => x.FirstName, "FirstName");
            Map(x => x.LastName, "LastName");
            Map(x => x.Email, "Email");
            Map(x => x.CreatedAt, "CreatedAt");

            Map(x => x.Addresses, "Addresses").CustomType<Blobbed<Dictionary<AddressType, Address>>>();
            Map(x => x.PhoneNumbers, "PhoneNumbers").CustomType<Blobbed<Dictionary<PhoneType, string>>>();
        }
    }

Now we can insert some data:

    using (var tx = session.BeginTransaction())
    {
        var customer = new Customer
        {
            FirstName = "Prentice",
            LastName = "Porter",
            Email = "banana3@test.com"
        };
        
        customer.Addresses.Add(AddressType.Home, new Address
        {
            Line1 = "13/187 Jones St",
            City = "Auckland",
            Country = "New Zealand",
            ZipCode = "0629"
        });

        customer.PhoneNumbers.Add(PhoneType.Mobile, "+64 27 551 443");
        customer.PhoneNumbers.Add(PhoneType.Home, "+64 9445 1982");

        session.SaveOrUpdate(customer);

        tx.Commit();
    }

Again, the data is inserted:

[![](/images/nhibernate-blobbing-3.png)](/images/nhibernate-blobbing-3.png)

*(Click on the image to see it fully)*

Only this data is serialized as JSON rather than JSV-format.

PhoneNumbers:

> {"Mobile":"+64 27 551 443","Home":"+64 9445 1982"}

And Addresses:

> {"Home":{"Line1":"13/187 Jones St","Line2":null,"ZipCode":"0629","State":null,"City":"Auckland","Country":"New Zealand"}}

If we query for the data:

    var customer = session.Get<Customer>(2);

![](/images/nhibernate-blobbing-4.png)

Just like OrmLite we get the object back just the same.

## Things to note ##

The custom user type in it's current state does not handle inherited objects. If you want to support it, then you can modify it to serialize and deserialize using the type information.

This can be done like so:

    JsonConvert.SerializeObject(x, 
        Formatting.None, 
        new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        
And

    JsonConvert.DeserializeObject<T>(val, 
        new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        
What this will do is include the type information on the serialized object:

> {"$type":"System.Collections.Generic.Dictionary2[[NHibernateJsonTest.AddressType, NHibernateJsonTest],[NHibernateJsonTest.Address, NHibernateJsonTest]], mscorlib","Home":{"$type":"NHibernateJsonTest.Address, NHibernateJsonTest","Line1":"13/187 Jones St","Line2":null,"ZipCode":"0629","State":null,"City":"Auckland","Country":"New Zealand"}}

## Why would you want to do this ##

To avoid unnecessary tables and mappings. The example above is a perfect example where we can remove the need for a table on data that is never searched against and is never related to anything else.

There's no need for joins or adding additional columns. We just map the object or collection to a single column and we are done, and our code knows no different.

Also, schema changes in blobs don't need DDL updates. If your model changes, you add new properties, or remove old properties, the blob will get updated next time you update your data. No more scripting off schema changes.

