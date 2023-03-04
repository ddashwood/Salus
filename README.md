# Salus
## Guarantee eventual consistency using Entity Framework in Microservices 

Named after the Roman goddess of safety and well-being, Salus ensures the safety
and well-being of your data in your Microservices-based system.

### Demo setup

Run the following command:

```
docker run -d --hostname salus-demo --name salus-demo-rabbit -p 15672:15672 -p 5672:5672 rabbitmq:3-management
```


*Full instructions coming soon*

### Known Issues

- The queue processor needs to order the items in the queue
- If there are items in the queue, does that mean we can't process any other items?
- Multiple updates need to be applied in the correct order
- The class name that is sent in the message is specific to the sender
- Having a different set of synchronous vs asynchronous senders can cause unintuitive
results - it's not possible to know which set of senders will be used without knowing
internals of Salus
