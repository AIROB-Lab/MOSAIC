# Software Design

Here are short descriptions of basic concepts in Software Engineering and Design. It's good to keep those in mind when working on a larger collaborative coding project, to keep your code clean and useful for others. Furthermore some Coding Patterns are explained later on, which are found in iMBlocks. 

![3](/images/code.jpg) 

## SOLID Principles in C#
The SOLID principles are like a set of important rules that help make software
easier to build and maintain. First introduced in 2000 by Robert C. Martin a.k.a Uncle Bob in
Design Principles and Design Patterns. They’re like guidelines that developers
follow to create flexible and long-lasting code. These principles are especially
useful for agile development, where things can change quickly. These principles
can save money and make sure that software projects stay healthy for a long
time.

Uncle Bob also popularized Clean Coding through his book Clean Code: A Handbook of Agile Software Craftmanship. A lot of his talks on Clean Coding and OOP are on YouTube, including reviews of bad Code. 

![3](/images/uncle_bob.jpg "Uncle Bob") 

Here are some benefits of using SOLID principles:
- Easier to Maintain: When you follow SOLID principles, your code becomes
  like building blocks. It’s easier to add new things or change existing
  ones without breaking everything.
- Less Complicated Code: SOLID principles help to keep things simple. By
  organizing code in a certain way, it becomes easier to understand and work
  with.
- More Flexible Code: You can add new features without messing up what’s
  already there. It’s like building on top of a strong foundation without
  having to rebuild everything underneath.
- Better Scaling:SOLID principles make sure your software can grow
  smoothly. They prevent the code from becoming a tangled mess as the
  project gets bigger.

The 5 principles of SOLID are:
- Single-responsibility principle
- Open-closed principle
- Liskov substitution principle
- Interface segregation principle
- Dependency inversion principle


### Single-responsibility principle

> “A class should only have a single responsibility, that is, only changes
> to one part of the software’s specification should be able to affect
> the specification of the class.” -Robert C. Martin a.k.a. Uncle Bob

The Single Responsibility Principle (SRP) is a rule that says each part of your
program should have just one job. Whether it’s a class, module, or function, it
should be responsible for only one thing and do it well. This makes it easy to
understand, change, and fix.

<img src="/images/srp.png" width="400">

Think of it like this: if you need to change something in your program, it should
only affect one specific part. Each class should have a single reason to change,
meaning you won’t mess up other things when you make updates.

Following SRP also helps with encapsulation, a fancy word for keeping things
private. When everything related to a job is in one place, it’s easier to hide the
details and control access to them. Adding a getter or setter method to a class
following SRP makes it nicely encapsulated.

Why does this matter? Well, if a part of your program breaks, you know exactly
where to look. You don’t have to search through a bunch of unrelated stuff. It
also makes your code easier to read because you only need to understand one
part at a time. So, SRP makes your code clean, understandable, and less likely
to cause headaches when you make changes.

Here's a bad example of what NOT to do:
```cpp
// does not follow SRP
public class RegisterService
{
  public void RegisterUser(string username)
  {
    if (username == "admin")
    throw new InvalidOperationException();
    SqlConnection connection = new SqlConnection();
    connection.Open();
    SqlCommand command = new SqlCommand("INSERT INTO [...]");//Insert user into database.
    SmtpClient client = new SmtpClient("smtp.myhost.com");
    client.Send(new MailMessage()); //Send a welcome email.
  }
}
```
Register User has three different jobs: register a user, connect to a database
and send an email. It should be split into three methods.

### Open-closed principle

>“Software entities … should be open for extension, but closed for modification.” Robert C. Martin

The Open-Closed Principle (OCP) might sound confusing, but it’s about making
your code adaptable yet stable. It suggests creating a basic thing (like a class
or function) that can be easily expanded without changing the original.

Here’s the simple idea:
- Open for Extension: Make your code open to adding new features without
  messing up what already works.
- Closed for Modification: Once something is working, keep it as is. Avoid
  changing the original code to add new features.
 
<img src="/images/ocp.jpg" width="400">

How do you do this? Use polymorphism. Have a basic thing (let’s call it
a parent) and make new, specialized versions (let’s call them children) with
extra features.The trick is, you can work on these specialized versions without
worrying about changing the original or other special versions. It’s like building
on a strong foundation without shaking everything. Why is this good? It keeps
your code stable, and you can add new stuff without causing trouble in the
existing code.

In iMBlocks you can see Block as a parent class and any class that inherits from
it as the children, adding specific features they need, while keeping the basic
functionalities of the Block class.

### Liskov substitution principle

> “Objects in a program should be replaceable with instances of their subtypes without altering the correctness of that program.” -Robert C. Martin

The Liskov Substitution Principle (LSP) created by Barbara Liskov and Jeannette
Wing is a rule that says if you have a class, you should be able to use any
of its subclasses without causing issues. In simple terms, a subclass must do
everything the parent class can do, plus some extra things if needed. 

- Replaceable Subclasses: Any new class you create should be easily replaceable
  with its parent class, meaning it can handle all the same tasks without
  causing errors.
- Consistent Behavior: If you add a new class (subclass), it should keep the
  behaviors of the original class while adding its unique features. It’s like
  having a common set of rules for all related classes.

Why is this helpful? It makes adding new subclasses faster and more reliable.
You can trust that they will work seamlessly with the existing code. You won’t
have to redo everything if you want to introduce a new subclass.
While some argue about its consistency in certain situations, in general, LSP
helps keep your code flexible and easy to expand. LSP is one of the harder principles to understand and follow while coding.

Bad example:
```cpp
class Program
{
  static void Main(string[] args)
  {
    Apple apple = new Orange();
    Debug.WriteLine(apple.GetColor());
  }
}

public class Apple
{
  public virtual string GetColor()
  {
    return "Red";
  }
}

public class Orange : Apple
{
  public override string GetColor()
  {
    return "Orange";
  }
}
```

This does not follow LSP because the Orange class could not replace the Apple
class without altering the program output. The GetColor() method is overridden
by the Orange class and therefore would return that an apple is orange.
Good Example:


```cpp
class Program
{
  static void Main(string[] args)
  {
    Fruit fruit = new Orange();
    Debug.WriteLine(fruit.GetColor());
    fruit = new Apple();
    Debug.WriteLine(fruit.GetColor());
  }
}

public abstract class Fruit
{
  public abstract string GetColor();
}

public class Apple : Fruit
{
  public override string GetColor()
  {
    return "Red";
  }
}

public class Orange : Fruit
{
  public override string GetColor()
  {
    return "Orange";
  }
}
```

### Interface segregation principle

> “Many client-specific interfaces are better than one general-purpose interface.” - Robert C. Martin

The Interface Segregation Principle (ISP) is about making sure that classes only have the behaviors they really need. In simple terms, a class should not be
burdened with methods it doesn’t use.

- Useful Behaviors Only: A class should only have methods that directly help it do its job. No unnecessary stuff.
- Splitting Large Methods: Break down big methods into smaller, specific ones. It’s like organizing your code into bite-sized pieces.

Why does this matter? It makes the program easier to understand and fix:

- Less Code Between Classes: Smaller methods mean less chance of mistakes when moving code around.
- Easier Debugging:Each method is responsible for a smaller set of tasks. If something goes wrong, you only need to check the smaller methods.
- Avoiding Bugs: If a class gets a method with behaviors it doesn’t support, it can lead to bugs. ISP helps prevent that.

In a nutshell, ISP keeps things neat and focused, making your code more readable and less prone to errors.

Bad example:

```cpp
// Not following the Interface Segregation Principle
public interface IWorker
{
  string ID { get; set; }
  string Name { get; set; }
  string Email { get; set; }
  float MonthlySalary { get; set; }
  float OtherBenefits { get; set; }
  float HourlyRate { get; set; }
  float HoursInMonth { get; set; }
  float CalculateNetSalary();
  float CalculateWorkedSalary();
}

public class FullTimeEmployee : IWorker
{
  public string ID { get; set; }
  public string Name { get; set; }
  public string Email { get; set; }
  public float MonthlySalary { get; set; }
  public float OtherBenefits { get; set; }
  public float HourlyRate { get; set; }
  public float HoursInMonth { get; set; }
  public float CalculateNetSalary() => MonthlySalary + OtherBenefits;
  public float CalculateWorkedSalary() => throw new NotImplementedException();
}

public class ContractEmployee : IWorker
{
  public string ID { get; set; }
  public string Name { get; set; }
  public string Email { get; set; }
  public float MonthlySalary { get; set; }
  public float OtherBenefits { get; set; }
  public float HourlyRate { get; set; }
  public float HoursInMonth { get; set; }
  public float CalculateNetSalary() => throw new NotImplementedException();
  public float CalculateWorkedSalary() => HourlyRate * HoursInMonth;
}
```

This program does not follow ISP because the FullTimeEmployee class does not
need the CalculateWorkedSalary() function, and the ContractEmployee class
does not need the CalculateNetSalary().

Better:

```cpp
// Following the Interface Segregation Principle
public interface IBaseWorker
{
  string ID { get; set; }
  string Name { get; set; }
  string Email { get; set; }
}

public interface IFullTimeWorkerSalary : IBaseWorker
{
  float MonthlySalary { get; set; }
  float OtherBenefits { get; set; }
  float CalculateNetSalary();
}

public interface IContractWorkerSalary : IBaseWorker
{
  float HourlyRate { get; set; }
  float HoursInMonth { get; set; }
  float CalculateWorkedSalary();
}

public class FullTimeEmployeeFixed : IFullTimeWorkerSalary
{
  public string ID { get; set; }
  public string Name { get; set; }
  public string Email { get; set; }
  public float MonthlySalary { get; set; }
  public float OtherBenefits { get; set; }
  public float CalculateNetSalary() => MonthlySalary + OtherBenefits;
}

public class ContractEmployeeFixed : IContractWorkerSalary
{
  public string ID { get; set; }
  public string Name { get; set; }
  public string Email { get; set; }
  public float HourlyRate { get; set; }
  public float HoursInMonth { get; set; }
}
```

### Dependency inversion principle

> “One should depend upon abstractions, [not] concretions.” - Robert C. Martin

The Dependency Inversion Principle (DIP) has two simple rules:

- No Direct Connections: High-level parts of your program (the big picture) shouldn’t be directly tied to low-level details (specifics). Instead, both should connect through more flexible abstractions, like interfaces. 
  
  Details Follow Abstractions: The specific nitty-gritty details (implementations) should depend on the broader abstractions. If you change the details, it shouldn’t mess up the big picture.

- Easy Changes: It makes your program more flexible. If you need to change something, you won’t have to redo the entire thing. Changes become more localized and less of a headache. 
  
  Refactoring Freedom: You can revamp the inner workings of your program without the user even
  noticing. This is known as refactoring and gives you the freedom to improve
  things behind the scenes without causing a fuss. 
  
In a nutshell, DIP helps keep
your code loosely connected, making it easier to manage and giving you room
to upgrade without causing chaos.


<img src="/images/uncle_bob_2.jpg" width="1000">


---



## Design Pattern

### Singelton 

Through a Singelton you ensure that a class only has one instance and a global
point to access it. In iMBlocks the classes Scheduler and Blocks use this pattern.

```cpp
public class Singleton
{
  static Singleton instance;
  // Constructor is 'protected'
  protected Singleton() {}

  public static Singleton Instance()
  {
    // Uses lazy initialization.
    if (instance == null)
    {
      instance = new Singleton();
    }
    return instance;
  }
}
```

If you want to try it out, here is an easy proof that s1 and s2 are the same
instance.

```cpp
public class Program
{
  public static void Main(string[] args)
  {
    // Constructor is protected -- cannot use new
    Singleton s1 = Singleton.Instance();
    Singleton s2 = Singleton.Instance();
    // Test for same instance
    if (s1 == s2)
    {
      Console.WriteLine("Objects are the same instance");
    }
    // Wait for user
    Console.ReadKey();
  }
}
```

### Interfaces
Interfaces in C# enable you to establish an agreement in your application. An interface defines properties, methods, and events, which are then implemented by classes. The interface itself does not offer implementations for the members it defines; it only specifies the members that must be supplied by classes that implement the interface.

Here are the key points you should know about Interfaces:
- Declaration: Interfaces are declared using the interface keyword.
- Implementation: A class or struct that implements an interface must provide concrete implementations of the interface's members.
- Inheritance: An interface can inherit from one or more other interfaces.
- Accessibility: Members of an interface are automatically public, and they cannot include any access modifiers.
Interfaces in C# enable you to establish an agreement in your application. An interface defines properties, methods, and events, which are then implemented by classes. The interface itself does not offer implementations for the members it defines; it only specifies the members that must be supplied by classes that implement the interface.

Basic Example of an Interface:
```cpp
public interface IAnimal
{
    void Eat();
    void Move();
}
```
Implementing an Interface in a Class:
```cpp
public class Dog : IAnimal
{
    public void Eat()
    {
        Console.WriteLine("Dog is eating.");
    }

    public void Move()
    {
        Console.WriteLine("Dog is running.");
    }
}
```