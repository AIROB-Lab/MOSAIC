# C# Basics and Resources 

This is a collection of explanations we usally hand out to new students in our lab. But it can also serve as an easy, quick overview and starting point for people without any C# and .NET experience.

C#  is a modern, object-oriented programming language developed by Microsoft as part of its .NET initiative. It is designed to be simple yet powerful, enabling developers to create various applications, from desktop and web applications to game development using platforms like Unity. With its foundation in the .NET Framework, C# allows developers to build applications that run on Windows. Still, thanks to the cross-platform .NET Core platform, it can also be used to develop applications that run on Linux and macOS. In the following is a very brief introduction to the basics of C#. For detailed Documentation and Tutorials on C# check out the Microsoft Learn documentation  <a href="https://learn.microsoft.com/en-us/dotnet/csharp/">here</a>. There you can also find the official <a href="https://learn.microsoft.com/en-us/dotnet/api/?view=netframework-4.8&preserve-view=true"> .NET API documentation</a>. Currently MOSAIC runs on the *.NET Framework 4.8* so make sure this one is selected when browsing the documentation. Great C# Tutorials can also be found <a href="https://www.w3schools.com/cs/index.php">here</a>.

Large Language Model Tools like ChatGPT can also be very helpful, just always check if the answers make actual sense and seem reasonable. <a href="https://codepal.ai/#">CODEPAL</a> is another very helpful tool, specialized on coding. It offers helpers like Code Reviewer, Code Explainer, Code Generator, Code Fixer, Bug Detector and many more. Definitely worth it to check out when your stuck or can't find a bug.

## IDE Setup

For the Development Environment Visual Studio Community 2022 is the preferred, free of charge solution, which can be downloaded <a href="https://visualstudio.microsoft.com/de/downloads/">here</a>. As an alternative C# support packages for Visual Studio Code are also available. In the Installer make sure to install at least .NET desktop development and Universal Windows Platform development components. If you plan to use Unity install the Game development with Unity component as well.

![5](/images/setup_1.png "") 
![5](/images/setup_2.png "") 

## Package Manager

Packages in .NET are easily managed through the NuGet Manager, which can be opened under Tools -> NuGet Package Manager -> Manage NuGet packages for Solution. Make sure following packages are installed to run MOSAIC:

- YamlDotNet (by Antoine Aubry)
- GraphX (by panthernet)
- MathNet.Numerics (by Christoph Ruegg, Marcus Cuda, Jurgen Van Gael)
- MathNet.Numerics.MKL.Win (by Christoph Ruegg, Marcus Cuda, Jurgen Van Gael)
- Microsoft.Windows.SDK.Contracts (by Microsoft)

By clicking Browse in the top left corner of the NuGet Manager you can search for missing packages and install them.

## C# basics

C# is a programming language that is object-oriented and strongly typed. It is built around the idea that "everything is an object". This principle is central to how C# programs are written and structured. The use of classes is fundamental to this approach. Classes are like blueprints that help create objects (which are instances of classes). It is essential to grasp this concept if you are starting to learn C#.

### Classes and Objects
In C#, a class is a structure that allows you to create your unique data types by combining variables of other types, methods, and events. A class functions as a template that defines the behavior and state that all the objects of the class share. An object is a particular instance of a class that is created in memory, and it can be used to access the class's methods and properties.

The typical structure of a C# Class is as follows:

```cpp
public class MyClass
{
    // Fields

    // Properties

    // Constructors

    // Methods

    // Events
}
```

A field ist a class-level variable, holding a value. They should be private and be used with a property, which encapsulates the private field. Properties have getter and setter functions to assign and retrieve the field value. Additional logic can be applied in the getter and setter functions.

```cpp
class Student
{
    // field
    private int id;

    //property
    public int StudentId
    {
        get { return id; }

        set 
        {
            if (value > 0)
                id = value;
        }
    }
}
```

If no additional logic is needed the Auto-implemented Property can be used. The complier will automatically generate the underlying  private field. 

```cpp
class Student
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```
Use PasacalCasing for class, methods and property names and camelCasing for local variables and method arguments, to be consistent with Microsoft .NET and easy readability. Typically no underscore are used except for private variables or fields, for example <code>private string _name;</code>.
```cpp
public class UserLog
{
    public void Add(LogEvent logEvent)
    {
        int itemCount = logEvent.Items.Count;
        // ...
    }
}
```
Formatting your code properly makes it easier to read for everyone else. In Visual Studio you can use the shortcuts: Ctrl/CMD + K + F for auto formatting a selection or Ctrl/CMD + K + D for auto formatting a whole document.

```cpp
public void Numbers(int number)
{
  //Good formatting
  if (number > 0)
  {
    Console.WriteLine(number);
  }
  //Bad formatting
  if (number < 0)
  {
  Console.WriteLine(number);
  }
}
```

Commenting your code in XML-style comments is necessary to compile API Documentations and is just general best practice. So please comment new code you write for MOSAIC so everybody knows whats going on. Here is an example
how a well commented class looks like. When you write /// before a class, field, property or method Visual Studio should automatically complete the XML-comment layout, you just need to fill in the descriptive text  in the right places. More information about XML comments in .NET can be found <a href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags">here</a>

```cpp
/// <summary>
/// Represents a simple bank account.
/// </summary>
/// <remarks>
/// This class can be used to manage the balance of a bank account,
/// allowing deposits and withdrawals.
/// </remarks>
public class BankAccount
{
    /// <summary>
    /// The account balance.
    /// </summary>
    private double _balance;

    /// <summary>
    /// Initializes a new instance of the <see cref="BankAccount"/> class.
    /// </summary>
    /// <param name="initialBalance">The initial balance of the account.</param>
    public BankAccount(double initialBalance)
    {
        _balance = initialBalance;
    }

    /// <summary>
    /// Deposits the specified amount into the account.
    /// </summary>
    /// <param name="amount">The amount to deposit.</param>
    /// <returns>The new balance after the deposit.</returns>
    public double Deposit(double amount)
    {
        _balance += amount;
        return balance;
    }

    /// <summary>
    /// Withdraws the specified amount from the account.
    /// </summary>
    /// <param name="amount">The amount to withdraw.</param>
    /// <returns>The new balance after the withdrawal.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the withdrawal amount exceeds the balance.</exception>
    public double Withdraw(double amount)
    {
        if (amount > _balance)
        {
            throw new InvalidOperationException("Withdrawal amount exceeds the balance.");
        }

        _balance -= amount;
        return _balance;
    }
}
```

## General Best Practices for C#

### Exception Handeling
Prevents your code from crashing and adding a meaningful error message to debug console,
or log-file will help with debugging. Try-catch statements are good practice
for this.

```cpp
public void NameCheck(string name)
{
  try
  {
    Name = name;
  }
  catch (Exception exception) { }
  {
    Debug.WriteLine("Your error Message");
    Debug.WriteLine(exception.Message);
  }
}
```

### <code>var</code> Keyword

Only use the var keyword for local variables when the type is obvious and
implicit from the right side.

```cpp
var myList = new List<string>();
```

### Ternary operator

A short-hand for if else. You can use it to replace simple if else statements.
Default pattern: variable = (condition) ? expressionTrue : expressionFalse;

This if else:

```cpp
int time = 20;
if (time < 18)
{
  Console.WriteLine("Good day.");
}
else
{
  Console.WriteLine("Good evening.");
}
```

Can be written like this:

```cpp
int time = 20;
string result = (time < 18) ? "Good day." : "Good evening.";
Console.WriteLine(result);
```

### Null-coalescing operator (??)

The null-coalescing operator (??) is like a shortcut for dealing with null values. It’s a simple way to check if a value is not null. If the value is not null, it gives you that value. If the value is null, it gives you a default value instead. So, it’s
like saying, “If the first thing is there, use it. If not, use the second thing as a
backup.” It’s a quick way to handle situations where you might have null values
and want to provide a fallback.

Default pattern: [value to test and return if not null] ?? [value to return if
tested value is null]

```cpp
int result = someNullableInt ?? defaultValue;
string name = somePossiblyNullString ?? "DefaultName";
```

### using Statement and IDisposable

In C#, objects may use managed or unmanaged resources, and it’s the client’s
responsibility to manage the object’s lifecycle. When the client sets an object to
null, it merely removes the reference from the stack, not from the heap. Garbage
collection, a non-deterministic process, eventually clears objects from memory.

C# addresses this by allowing clients to explicitly release resources using the
IDisposable interface. This interface has a Dispose method where the object
implements code to release resources. Clients can then call Dispose to release
resources, providing a controlled way to manage resource cleanup.

Why should you use it? The “using” statement in C# offers a convenient way
to explicitly invoke the Dispose method. By instantiating an object within the
“using” block, the Dispose method is automatically called at the block’s end.

Key features of the “using” statement include:
- Managed Scope: The “using” statement manages the object’s scope. Upon exiting
  the block, Dispose is invoked, ensuring the release of all resources, making
  the object unavailable thereafter.
- Read-Only Instantiation: Objects instantiated within a “using” statement are
  read-only, preventing modification or reassignment. This restriction guarantees
  that the Dispose method is called on the instantiated object.
- Dispose Method Assurance: The “using” statement guarantees that the Dispose
  method will always be called, regardless of whether exceptions occur during its
  execution.

How to use it:

```cpp
// reading in data from a file
using (var sr = new StreamReader(Path))
```

### Asynchronous Programming with async/await

The async and await keywords in C# provide a simplified way to write asynchronous
code, making it easier to manage tasks that might take time to complete,
such as file I/O operations, database queries, or network requests, without
blocking the main thread.
- async: This modifier is used to define an asynchronous method. An asynchronous
    method can perform tasks without blocking the calling thread,
    allowing other operations to continue.
- await: This operator is used to asynchronously wait for a task to complete.
  It tells the compiler that the following method or operation can be awaited,
  allowing the calling method to continue execution until the awaited task
  finishes.

Example: Asynchronous File Reading

```cpp
using System;
using System.IO;
using System.Threading.Tasks;
class Program
{
  static async Task Main(string[] args)
  {
    Console.WriteLine("Starting file reading asynchronously...");
    // Specify the path of the file to read
    string filePath = @"C:\path\to\your\largefile.txt";
    try
    {
      // Start reading the file asynchronously, waiting for the results
      string fileContent = await ReadFileAsync(filePath);
      // Display the content of the file
      Console.WriteLine($"File content: {fileContent}");
    }
    catch (Exception ex)
    {
      // Handle exceptions if any
      Console.WriteLine($"An error occurred: {ex.Message}");
    }
    Console.WriteLine("File reading completed.");
  }
  // Asynchronous method to read a file
  static async Task<string> ReadFileAsync(string filePath)
  {
    using (StreamReader reader = new StreamReader(filePath))
    {
      // Asynchronously read the entire file content
      return await reader.ReadToEndAsync();
    }
  }
}
```

- the Main method is marked as async, allowing asynchronous operations
  inside it
- inside the Main method, we call the ReadFileAsync method asynchronously
  using the await keyword
- the ReadFileAsync method reads the file content asynchronously using
  StreamReader.ReadToEndAsync
- once the file content is read, the content is returned as a string, and the
  Main method continues execution, displaying the file content.

If you just want to start an asynchronous Task, which your Main function does
not need to wait on, you do not need to mark you Main function as async. You
can use Task.Run() to run an asynchronous method on a background thread
from the thread pool.

```cpp
{
  // Perform some synchronous operations first
  Console.WriteLine("Starting application...");
  // Call an asynchronous method but don't await it immediately
  Task.Run(() => SomeAsyncMethod());
  // Continue with other synchronous operations
  Console.WriteLine("Doing something else...");
}
static async Task SomeAsyncMethod()
{
  // Simulate some asynchronous work
  await Task.Delay(1000);
  Console.WriteLine("Async operation completed.");
}
```

Async Best Practices: 
- Avoid using async void unless it’s for event handlers.
- Always prefer Task.Run for CPU-bound operations to offload them from the
  main thread. 
- Be cautious with exception handling; ensure you handle exceptions
  appropriately, especially when awaiting tasks.

### Comparing a value to an empty <code>string</code>

Best practice is to use String.Empty instead of " ", to make clear it the empty
string comparison is intentionally and improve readability.

```cpp
if(name==String.Empty)
```

### String comparisons to user input 

To ensure case-insensitivity, user inputs are converted to upper- or lowercase.
This also eliminates case-handling logic and hence is more readable.

```cpp
if(name.ToLower() == "Joe")
if (name.ToUpper() == "Joe")
```

### Enums for discreet values

Instead of numbers and strings, enums can be used to indicate discreet values,
making code typesafe, more readable and efficient. Enum is special “class”
representing a group of constants, hence unchangeable, readonly variables.

```cpp
class Program
{
  enum Level
  {
    Low,
    Medium,
    High
  }

  static void Main(string[] args)
  {
    Level myVar = Level.Medium;
    Console.WriteLine(myVar);
  }
}
```
### AND, OR Operations

Use the short-circuit <code>&&</code> for and operations and <code>||</code> for or operations for better
performance.

```cpp
public void Numbers(int number)
{
  if (number > 2 && number < 4)
  {
    Console.WriteLine(number);
  }
}
```