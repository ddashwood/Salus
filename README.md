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

- Multiple updates need to be applied in the correct order even if they come out of order
- The class name that is sent in the message is specific to the sender
