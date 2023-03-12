# Salus
## Guarantee eventual consistency using Entity Framework in Microservices 

Named after the Roman goddess of safety and well-being, Salus ensures the safety
and well-being of your data in your Microservices-based system.

### *Status*

This project is still being built. **Do not use** until it is completed!

### Demo setup

1. Install Docker and ensure it is running
2. Run the following command to start RabbitMQ:

```
docker run -d --hostname salus-demo --name salus-demo-rabbit -p 15672:15672 -p 5672:5672 rabbitmq:3-management
```

Note: neither Docker nor RabbitMQ are required - you can make Salus work with any messaging system you like.
However, the demo programs make use of RabbitMQ for messaging, and using Docker is an easy way of setting up the demo.

3. Load the solution in Visual Studio. Run SalusExampleParent _and_ SalusExampleChild. (You can right-click on a project
and select Debug/Start New Instance to run the second project.)

In the parent application, you will see a form which allows you to add, edit or delete rows of very basic data.

Whatever changes you make in the parent application will be mirrored almost immediately in the child application. The
two applications are using entirely different instances of SqLite to store their data - the databases are not connected
at all. But every change that is made in the parent's database is notified to the child, and the child is able to update
its database.

**Here comes the clever bit.** Stop either RabbitMQ, or the child application, or both. Then continue to make changes
in the parent. When you start RabbitMQ and the child application, once everything is up and running again the changes will
be passed onto the child. Salus provides a very simple means of adding resiliancy - so that if your message system or
one of your microservices is unavailable, your data still regains eventual consistency!

And of course if you stop the parent from running, the child will continue to run, and can access its own copy of the 
data independent of the parent.

### Instructions

#### In the parent

1. Create a DbContext in the same way as usual, except:

- Inherit from SalusDbContext instead of DbContext
- Apply the `[SalusSourceDbSet]` attribute to any DbSet that you want Salus to monitor
2. Create a Message Sender, by creating a class which implements IAsyncMessageSender. In here, you need to add a single
method which sends messages. You can use whatever messaging technology you like in here (the example uses RabbitMQ).
You can add whatever routing or other instructions you need.
3. Register with dependency injection using the following code:

```csharp
services.AddSalus<MyContext>(new MessageSender(), salusOptions => 
{
    // There are a variety of options you can put here - see demo for examples
},
contextOptions =>
{
    // Put your DbContext options here, the same way you normally would
});
```

4. Use like any other DbContext!

#### In the child

1. Create a DbContext in the same way as usual, except:

- Inherit from SalusDbContext instead of DbContext
- Apply the `[SalusDestinationDbSet]` attribute to any DbSet that you want Salus to write to
2. Register with dependency injection using the following code:

```csharp
services.AddSalus<MyContext>(salusOptions => 
{
    // Probably not needed if you are only receiving Salus data, not sending
},
contextOptions =>
{
    // Put your DbContext options here, the same way you normally would
});
```

3. Add whatever code you need to receive messages from your messaging system. When you receive a message, call:

```csharp
context.Apply(message);
```

#### Matching tables in the parent and the child

By default, Salus looks to find a DbSet\<T\> in the child where the name of class T matches the name of the type used in the DbSet\<T\> in the parent.

If you want to use classes of different names in the parent and the child, you have three options:

1. In the parent, use `[SalusSourceDbSet(SalusName = "DestinationClassName")]`. This is probably not recommended as it creates
a tight coupling between the parent and the child

2. In the child, use `[SalusDestinationDbSet(SalusName = "SourceClassName")]`
3. In both the parent and the child, you can set the `SalusName` as shown in 1 and 2, but make sure they match in both the parent
and the child.

### Known Issues

This project is a work in progress. There are multiple things which are not tested, and may or may not work. This
list includes (but is not limited to):

- Multiple updates need to be applied in the correct order even if they come out of order (e.g. from a distrubted
setup, or if the messaging service sends messagesd out of order)
- DbSet.RemoveRange() - if the range has not been realized yet, the entities may not be tracked - needs testing to
see if this works
- Multiple related tables being updated at the same time may not work if the updates happen out of order - needs testing

### Testing

There is a test project attached to the solution. The tests are generally of the form of integration tests, rather than unit tests,
because testing the code in Salus is pointless unless the way it integrates with Entity Framework is as expected. Tests use 
SqLite In Memory databases, and so should be able to run in the kind of time you'd expect from unit tests.