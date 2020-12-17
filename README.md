# MyMicroservices
There is two microservices using .NET 5.0.  I have extended the microservices using CQRS, docker and docker-compose, RabbitMQ.
# The Microservices Responsibilities
Our two microservices should satisfy the following requirements:

* Implement a Customer API with the following methods: create customer, update customer
* Implement an Order API with the following methods: create order, pay order, get all orders which had already been paid
* When a customer name is updated, it should also be updated in the Order API
* The APIs should not directly call each other to update data
* .NET 5.0 Web API with DI/IoC
* Communication between microservices should be implemented through some kind of queue
* Use DDD and CQRS approaches with the Mediator and Repository Pattern

To keep it simple, I have useed an in-memory database (Microsoft.EntityFrameworkCore.InMemory).

# Structure of the Microservice
I created a solution for each microservice. Both microservices have exactly the same structure, except that the order solution has a Messaging.Receive project and the customer solution has a Messaging.Send project. I have used these projects to send and receive data using RabbitMQ.

I've placed everything like controllers or models in a v1 folder, to implement versioning. If I want to extend my feature and it is not breaking the current behavior, I will extend it in the already existing classes. If my changes were to break the functionality, I will create a v2 folder and place the changes there. With this approach, no consumer is affected and they can implement the new features whenever they want or need them.


# The API Project
The API project is the heart of the application and contains the controllers, validators, and models as well as the startup class in which all dependencies are registered. I have tried to keep the controller methods as simple as possible. They only call different services and return a model or status to the client. They don’t do any business logic. The `_mediator.Send` is used to call a service using CQRS and the Mediator pattern. Also To validate the user input, I've used the NuGet `FluentValidations` and a validator per model. I've tried to keep things simple and only validate that the first and last name has at least two characters and that the age and birthday are between zero and 150 years. But in a real project, we definitely need to add more rules.  

# The Data Project
The Data project contains everything needed to access the database. I've used Entity Framework Core, an in-memory database and the repository pattern. We can change it to a normal database easily in a real project though. All that we need to do, in order to use a normal database instead, is changing the following line:

    services.AddDbContext<CustomerContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));  
To this:
    services.AddDbContext<CustomerContext>(options => options.UseSqlServer(Configuration["Database:ConnectionString"]));  
# The Domain Project
The Domain project contains all entities and no business logic. In our microservices, this is only the `Customer` or `Order` entitities.
# The Messaging.Send Project
This project contains everything we need to send `Customer` objects to a RabbitMQ queue.
# The Service Project
The Service project is split into Command and Query. This is how CQRS separates the concerns of reading and writing data. Actually, commands are used to write data and queries to read data. A query consists of a query and a handler. The query indicates what action should be executed and the handler implements this action. The command works with the same principle.
# Tests
For Tests, I have used the xUnit, FakeItEasy, and FluentAssertions.
# Why we need to use a Queue to send data?
I've used the RabbitMQ as it describes itself as the most widely deployed open-source message brokera. There are a couple of reasons why using a queue instead of directly sending data from one microservice to the other one is better:
* Higher availability and better error handling
* Better scalability
* Share data with whoever wants/needs it
* Better user experience due to asynchronous processing

Actually, I've created a new project in the CustomerApi solution called CustomerApi.Message.Send. Then, I've installed the RabbitMQ.Client NuGet package and created the class CustomerUpdateSender in the project. I want to publish my Customer object to the queue every time the customer is updated. Therefore, I created the SendCustomer method which takes a Customer object as a parameter. In the other hands, In the OrderApi solution, I've created a new project called OrderApi.Messaging.Receive, installed the RabbitMQ.Client NuGet and created a class called `CustomerFullNameUpdateReceiver`. This class inherits from the `BackgroundService` class and overrides the `ExecuteAsync` method. In this project, we have to constantly check the queue if there are new messages and if so, process them. The .NET Core comes really handy here as it provides the abstract class `BackgroundService` which provides the method `ExecuteAsync` for this purpose. 

# The Application has been Dockerized
Microservices became so popular because they can be easily deployed using Docker. The first step, is running the RabbitMQ in Docker but before that we will need to  download the Docker Desktop for Windows which could be found [here](https://docs.docker.com/docker-for-windows/). After we've installed the Docker, we need to run the following lines in Powershell:

    docker run -d --hostname my-rabbit --name some-rabbit -e RABBITMQ_DEFAULT_USER=user -e RABBITMQ_DEFAULT_PASS=password rabbitmq:3-management
    docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
Actually, these two lines download the RabbitMQ Docker image, start it as a container and configure the ports, the name, and the credentials. After RabbitMQ is started, we can navigate to localhost:15672 and login with guest as user and guest as password. At this moment, if we navigate to the Queues tab and we will see that there is no queue yet. Now we can start the `OrderApi` and the `CustomerApi` projects. After that, the CustomerQueue will be created and we can see it in the management portal. Then, we can go to the Put action of the `CustomerApi` and update a customer. After we sent the update request, we can go back to the RabbitMQ management portal and we will see that a message has been published to the queue and also a message has been consumed.

I've also dockerized my microservices and have created the Docker containers which can be run anywhere as long as Docker is installed. Worth mentioning, I've selected the Linux as OS for the containers. We can see the Dockerfile in each project which is a set of instructions to build and run an image.

# Test the Dockerized Application

After adding the Docker support to our application, we should be able to select the Docker as a startup option in Visual Studio. When we select the Docker for the first time, Visual Studio will run the Dockerfile, therefore build and create the container. This might take a bit because the images for the .NET Core runtime and SDK need to be downloaded. After the first download, they are cached and can be quickly reused. After that we can use the command `docker ps` in PowerShell to make sure they are running inside a Docker container. As an another option we can also use the `tag`, `push` and `pull` commands to upload and download our images to Docker Hub.

# The Docker-Compose File

The Docker-compose is a yml file in which we can set up all the containers our application needs to run. Simplified, it executes several docker run commands after each other. To execute our compose file, we need to open the Powershel, and navigate to the location of our file. I have provided the compose file in the root folder. Once we navigated to our file, we can use the following command: `docker-compose up -d`. The -d parameter executes the command detached. This means that the containers run in the background and don’t block our Powershell window. Again, to prove that all three containers are running, we can use the `docker ps` command.

Another great feature of docker-compose is, that we can now stop all of our application with a single command: docker-compose down.

# Strengths and Weaknesses
Every technology and tool which we are using everyday, has it's own advantages and disadvantages for sure. A few I would like to highlight regarding this application is:

**Advantages of CQRS:**
* Separation of Concern, therefore simpler classes and models. Segregating the read and write sides can result in models that are more maintainable and flexible. Most of the complex business logic goes into the write model. The read model can be relatively simple.
* Security. It's easier to ensure that only the right domain entities are performing writes on the data.
* Better scalability: CQRS allows the read and write workloads to scale independently, since we can have a microservice only for queries and one only for commands. Reads occur often way more often than writes.
* Better performance as we can use a database for reading and a database for writing. We could also use a fast cache like Redis for the reading.

**Disadvantages of CQRS:**
* Not applicable in all projects: CQRS brings some complexity to our system and especially simple applications that do only basic CRUD operations shouldn’t use CQRS.

**Advantages of Mediator Pattern:**
* Less coupling: Since the classes don’t have dependencies on each other, they are less coupled.
* Easier reuse: Fewer dependencies also helps to reuse classes.
* Single Responsibility Principle: The services don’t have any logic to call other services, therefore they only do one thing.
* Open/closed principle: Adding new mediators can be done without changing the existing code.

**Disadvantages of Mediator Pattern:**
The mediator can become such a crucial factor in our application that it is called a “god class”. There is some solution around however, to prevent a Mediator from becoming a God Object, for instance [this](https://softwareengineering.stackexchange.com/questions/369690/how-to-prevent-a-mediator-from-becoming-a-god-object) one could be useful.

