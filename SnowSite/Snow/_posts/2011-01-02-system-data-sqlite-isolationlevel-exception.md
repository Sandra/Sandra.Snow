---
layout: post
title: System.Data.SQLite isolationLevel Exception
---

I introduced SQLite to our Unit Testing at work to aid with testing the stuff written with NHibernate, most of our repositories are rather simple but some of them require some specific criteria that it would be nice to test our queries work.

The problem is some of the queries have a transaction with the `IsolationLevel` as `ReadUncommitted`.

Everything works perfectly fine until it comes to testing, the problem is SQLite does not support anything other than `Serializable` and `ReadCommitted`.

I spent a while trying to see if there was a way to have an interceptor for NHibernate to capture the `BeginRequest` and replace the isolation level with something that would work, when that failed I took a look at extending the SQLite dialect but that just became confusing.

In the end I reflected the `System.Data.SQLite` assembly (before i downloaded the sourcecode) to see what was happening when `BeginTransaction` was being called.

There's two places it checks, the first is in `SQLiteConnection` under `BeginTransaction`:

    if (isolationLevel != IsolationLevel.Serializable && isolationLevel != IsolationLevel.ReadCommitted)
        throw new ArgumentException("isolationLevel");

<!--excerpt-->

The second is in the same file under Open:

    if (_defaultIsolation != IsolationLevel.Serializable && _defaultIsolation != IsolationLevel.ReadCommitted)
        throw new NotSupportedException("Invalid Default IsolationLevel specified");

The second one is only if you've configured SQLite to have a default isolationLevel, so I've changed both.

So before the change, if i ran the following code (to demonstrate the scenario):

    [TestMethod] 
    public void SQLite_WithTransction_ShouldNotThowException() 
    { 
        object id;
        
        using (var tx = _session.BeginTransaction(IsolationLevel.ReadUncommitted))
        { 
            id = _session.Save(new Test 
            { 
                Name = "Test" 
            });

            tx.Commit(); 
        }

        Assert.AreEqual(1, Convert.ToInt32(id)); 
    }

When the test runs, the following error is thrown:

> NHibernate.TransactionException: Begin failed with SQL exception -> System.ArgumentException: isolationLevel

Now if I update the two exceptions:

    if (_defaultIsolation != IsolationLevel.Serializable && _defaultIsolation != IsolationLevel.ReadCommitted)
        _defaultIsolation = IsolationLevel.ReadCommitted;
        
And

    if (isolationLevel != IsolationLevel.Serializable && isolationLevel != IsolationLevel.ReadCommitted)
        isolationLevel = _defaultIsolation;
        
Compile and all that stuff...

Now when I run the test:

![](/images/sqlite-exception-1.png)

Problem solved, now I can run unit tests against repositories that happen to use Isolation Levels not supported by SQLite.

[System.Data.SQLite.ForTesting.zip](/stuffz/System.Data_.SQLite.ForTesting.zip)

Attached is the assembly if anyone else wants to use it for their tests.
