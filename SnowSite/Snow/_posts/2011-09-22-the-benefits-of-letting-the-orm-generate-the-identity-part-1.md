---
layout: post
title: The benefits of letting the ORM generate the Identity (part 1)
category: NHibernate
---

One thing I've learnt is that letting the database generate the identity for you is a bad thing. It always annoyed me that Oracle never gave a feature like [AUTO_INCREMENT](http://dev.mysql.com/doc/refman/5.0/en/example-auto-increment.html) in MySQL or [IDENTITY](http://msdn.microsoft.com/en-us/library/aa933196(SQL.80).aspx) in SQL Server. I never understood, when inserting data how do I give it an Id?

Well one of the benefits of ORMs such as NHibernate is we can generate the identity our-self, or rather, the ORM can generate it so we don't rely on the database. This also plays a major part in our code base when we insert a graph or batch of data and how the identity is added to our object.

Ayende [recommend avoiding identity](http://ayende.com/blog/3915/nhibernate-avoid-identity-generator-when-possible).

The thing with using SQL Server"s identity is that we need to select the identity back out after we do an insert. Not only that, when using [NEWID()](http://msdn.microsoft.com/en-us/library/ms190348.aspx)/[NEWSEQUENTIALID()](http://msdn.microsoft.com/en-us/library/ms189786.aspx) there is no way to select the value back other than using all the fields in a select in order to get the GUID relates to the record with all those values matching.

For example:

Given this rather simple table using IDENTITY.

    CREATE TABLE People
    (
        Id int NOT NULL IDENTITY (1, 1) PRIMARY KEY,
        FirstName nvarchar(100) NOT NULL,
        Surname nvarchar(100) NOT NULL
    )

<!--excerpt-->

And I'll demo with NEWID() as well.

    CREATE TABLE Fruit
    (
        Id uniqueidentifier NOT NULL PRIMARY KEY DEFAULT (NEWID()),
        Name nvarchar(100) NOT NULL
    )
    
<span class="note">**Note:** I'm unaware of anyway to get NHibernate to use NEWSEQUENTIALID()</span>

We map these in NHibernate like so:

    public class PersonMap : ClassMap<Person>
    {
        public PersonMap()
        {
            Table("People");
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.FirstName);
            Map(x => x.Surname);
        }
    }

    public class FruitMap : ClassMap<Fruit>
    {
        public FruitMap()
        {
            Table("Fruit");
            Id(x => x.Id).GeneratedBy.GuidNative();

            Map(x => x.Name);
        }
    }

If we insert data into People and Fruit like so:

    using (var tx = session.BeginTransaction())
    {
        var person = new Person
        {
            FirstName = "Phillip",
            Surname = "Haydon"
        };
        
        var fruit = new Fruit
        {
            Name = "Apple"
        };

        session.SaveOrUpdate(person);
        session.SaveOrUpdate(fruit);

        tx.Commit();
    }

We get the following statement's run:

    - statement #1
    begin transaction with isolation level: Unspecified
    
    - statement #2
    INSERT INTO People
                (FirstName,
                 Surname)
    VALUES      ('Phillip' /* @p0 */,
                 'Haydon' /* @p1 */);

    select SCOPE_IDENTITY()

    - statement #3
    select newid()

    - statement #4
    INSERT INTO Fruit
                (Name,
                 Id)
    VALUES      ('Apple' /* @p0 */,
                 '3411c820-f9cc-4385-97a1-31cf7e7c612c' /* @p1 */)

    - statement #5
    commit transaction

What's interesting is for the Identity, we have to select the SCOPE_IDENTITY() back after the insert so that we can populate the Person object, and for the Fruit object, we have to select NEWID() first as a separate statement, then add it to the business object, and commit it.

This round-trip to the database in order to get the GUID first before doing the insert is completely unnecessary, not to mention has a performance impact.

Inserting 50,000 items for each, with the batch-size set to 50, yields the following:

<table>
  <tr>
    <td>IDENTITY</td>
    <td>28951</td>
  </tr>
  <tr>
    <td>NEWID</td>
    <td>30241</td>
  </tr>
</table>

(value in milliseconds)

<span class="note">**Note:** These benchmarks are quick-nasty benchmarks and were only run once.</span>

The interesting thing is neither IDENTITY or NEWID batched any of the insert statements together, they were all issued as separate statements.

(re-run the test inserting 3 to show SQL output)

The 'Person' insert looks like this:

    - statement #1
    begin transaction with isolation level: Unspecified
    
    - statement #2
    INSERT INTO People
                (FirstName,
                 Surname)
    VALUES      ('Phillip0' /* @p0 */,
                 'Haydon' /* @p1 */);

    select SCOPE_IDENTITY()

    - statement #3
    INSERT INTO People
                (FirstName,
                 Surname)
    VALUES      ('Phillip1' /* @p0 */,
                 'Haydon' /* @p1 */);

    select SCOPE_IDENTITY()

    - statement #4
    INSERT INTO People
                (FirstName,
                 Surname)
    VALUES      ('Phillip2' /* @p0 */,
                 'Haydon' /* @p1 */);
                 
    select SCOPE_IDENTITY()

    - statement #5
    commit transaction

Each insert has to be done 1 by 1, since it needs to select the identity back.

The 'Fruit' table on the other hand:

    - statement #1
    begin transaction with isolation level: Unspecified
    
    - statement #2
    select newid()

    - statement #3
    select newid()

    - statement #4
    select newid()

    - statement #5
    INSERT INTO Fruit
                (Name,
                 Id)
    VALUES      ('Apple0' /* @p0_0 */,
                 '269bc638-74b4-4568-85d1-45b6e537fcbd' /* @p1_0 */)

    INSERT INTO Fruit
                (Name,
                 Id)
    VALUES      ('Apple1' /* @p0_1 */,
                 'fc848779-b173-4c31-b8b6-0a7735c0c2dc' /* @p1_1 */)

    INSERT INTO Fruit
                (Name,
                 Id)
    VALUES      ('Apple2' /* @p0_2 */,
                 '232c8971-18c7-486d-9152-26c969c3b632' /* @p1_2 */)

    - statement #6
    commit transaction

It select 50,000 GUIDs first, then it issues all the insert statement's in batches of 50.

Now lets look at HiLo and GuidComb (GuidComb is a Sequencial Guid, but NH also allows normal Guids), two ways of generating identities in the ORM rather than the database.

The tables are the same as before, except they don't have 'IDENTITY' or a Default Value.

The mappings have been updated to:

    Id(x => x.Id).GeneratedBy.HiLo("100");

And

    Id(x => x.Id).GeneratedBy.GuidComb();
    
Running a single insert for both results in:

    - statement #1
    begin transaction with isolation level: Unspecified
    
    - statement #2
    Reading high value: 
    select next_hi
    from   hibernate_unique_key with (updlock, rowlock)

    - statement #3
    Updating high value: 
    update hibernate_unique_key
    set    next_hi = 2 /* @p0 */
    where  next_hi = 1 /* @p1 */

    - statement #4
    INSERT INTO People
                (FirstName,
                 Surname,
                 Id)
    VALUES      ('Phillip' /* @p0_0 */,
                 'Haydon' /* @p1_0 */,
                 101 /* @p2_0 */)

    - statement #5
    INSERT INTO Fruit
                (Name,
                 Id)
    VALUES      ('Apple' /* @p0_0 */,
                 '3229618e-bd8a-45ae-8ad5-9f660016980d' /* @p1_0 */)

    - statement #6
    commit transaction

Besides NHibernate getting the first Hi value for use in the HiLo algorithm, both insert statement did not require selecting or generating any identity, it was all done in NHibernate.

This makes inserting 3 'Person' much more efficient:

    - statement #1
    begin transaction with isolation level: Unspecified
    
    - statement #2
    Reading high value: 
    select next_hi
    from   hibernate_unique_key with (updlock, rowlock)

    - statement #3
    Updating high value: 
    update hibernate_unique_key
    set    next_hi = 3 /* @p0 */
    where  next_hi = 2 /* @p1 */

    - statement #4
    INSERT INTO People
                (FirstName,
                 Surname,
                 Id)
    VALUES      ('Phillip0' /* @p0_0 */,
                 'Haydon' /* @p1_0 */,
                 202 /* @p2_0 */)

    INSERT INTO People
                (FirstName,
                 Surname,
                 Id)
    VALUES      ('Phillip1' /* @p0_1 */,
                 'Haydon' /* @p1_1 */,
                 203 /* @p2_1 */)

    INSERT INTO People
                (FirstName,
                 Surname,
                 Id)
    VALUES      ('Phillip2' /* @p0_2 */,
                 'Haydon' /* @p1_2 */,
                 204 /* @p2_2 */)

    - statement #5
    commit transaction

3 insert's done as a single batch statement.

And inserting 3 Fruit:

    - statement #1
    begin transaction with isolation level: Unspecified
    
    - statement #2
    INSERT INTO Fruit
                (Name,
                 Id)
    VALUES      ('Apple0' /* @p0_0 */,
                 'db902160-edbb-49c7-bf52-9f660018299a' /* @p1_0 */)

    INSERT INTO Fruit
                (Name,
                 Id)
    VALUES      ('Apple1' /* @p0_1 */,
                 '5e852528-3a6f-41d2-a6b1-9f660018299a' /* @p1_1 */)

    INSERT INTO Fruit
                (Name,
                 Id)
    VALUES      ('Apple2' /* @p0_2 */,
                 '2f63c6e8-e595-4393-ad15-9f660018299a' /* @p1_2 */)

    - statement #3
    commit transaction

Again, 3 insert's done as a single batch statement.

If we run 50,000 inserts again:

<table>
  <tr>
    <td>HiLo</td>
    <td>9287</td>
  </tr>
  <tr>
    <td>GuidComb</td>
    <td>9060</td>
  </tr>
</table>

That is over 3 times faster!

So as you can see, for just doing batch inserting, with a full Session (rather than stateless) and allowing the ORM to generate identities, we can significantly improve performance.

