<h1>What is it about</h1>

Once the system has been installed and sanity-checked, you are ready to study the code, but first, some background and motivation.<br>

Our whole work is about user studies, meaning that we need to have people (participants) do things (tasks) in front of a monitor, in VR or in an actual physical setting (experiment), in a precisely structured way (experimental protocol), while we record data from and about them. Recording good data is paramount, since we then analyze the data statistically and draw conclusions about our own future work. So, we need a software suite able to:

- *Carefully timestamp* and *dump data*, mostly signals read from physical devices but not only that. Remember: most OSs are not real-time (Window definitely is ***not!***), meaning that no-one can assure you   that a specific loop will run exactly N times a second; yet, if you have an accurate „clock“, you can timestamp your events and observations, and at least know whether something is going wrong, and how much it is going wrong.
- *Graphically display* things on a GUI and possibly use it to interact with the system and the participant \- start and stop tasks, change the experimental conditions, show emergency lights, and display the status of the system.
- *Issue control commands* to any device, e.g., virtual objects in a virtual world, physical devices such as arm/hand prostheses and exoskeletons, objects and gadgets appearing on a screen.

Since each device (including both sensors, robotics artifacts and the GUI itself!) runs at its own sampling rate / control rate, we need a highly multithreaded programming environment; as well, we need to be able to easily create GUIs for the experimenter and/or the participant. We usually do not need real-time programming. Plus, and this is a *fundamental point*, our code needs to be ***readable, easy to use, quickly grasped by the newcomers***, since this is a research environment with teaching purposes, and not a company. We need no superfast, contrived software that only hackers can understand! Within reasonable limits, “clear and easily modifiable but slow” is better than “fast but jumbled and obscure”.

> [!IMPORTANT]
> Code needs to be readable, easy to use and quickly understandable!

> [!NOTE]
> This is why *Mathnet.Numerics* is our mathematical library of choice: you can definitely find faster libraries around, but this one is readable. My favorite example: the well-known formula for Ridge Regression, $w=[(X^T X+λI)]^(-1) X^Ty$, is written in C# using Mathnet as follows:<br><br>
> <center><code>w = (X.Transpose()*X+lambda*I).Inverse()*(X.Transpose()*y);</code></center><br><br>
> As you can see, the resemblance between math notation and code is remarkable. As far as speed (efficiency) is concerned, Mathnet comes with a few precompiled “native” DLLs, one of which is optimized for the Intel processors. Explicitly loading this DLL at the beginning of our programs (see, e.g., the very first lines of <code>Program.cs</code> in our template project) achieves a very reasonable speed.


The best OS/language combination has shown to be C\# under Windows (Visual Studio), that is why we use it – it integrates well all kinds of device drivers, and it allows for the easy creation of GUIs; plus, it has built-in multi-threading and event-driven features. On top of this, one of the most used VR scenario creation utilities, Unity, precisely accepts C\# as its programming language and happily interfaces with iM. Last but not least, it is reasonably easy to plug python, C++ or Java code in C\#. C\# is fun – if you are already familiar with, e.g., Java or Visual Basic, then C\# is just a few steps away. If you are familiar with C++, you will find that C\# is a simpler, easier, hassle-free version of C++ which retains most of C++’s benefits and perks. <br>

In the past, *interactiveMyocontrol* has shown to be effective at providing all this, so we kind of stuck with it for a few years. Why a new implementation then? The reason is that the old implementation had become cluttered with additions and patches, some of which made necessary by the lack of a founding structure or concept if you prefer. iM solves this issue via a simple concept: every operation you need is an instance of an abstract C\# class called <code>Block</code>. In its deepest and true essence, each <code>Block</code> is an independent black box, asynchronously receiving stuff from other <code>Block</code>s, somehow processing the stuff, then asynchronously making the result of the computation available to any other <code>Blocks</code> needing it.<br>

In C\# terms, the class called <code>Block</code>
-	Subscribes to the *event handler* of its input <code>Block</code>s, meaning that it has a call-back method called <code>OnNewInput(sender,value)</code> which gets called each time one of the block’s input blocks (the sender) produces a new value. OnNewInput can perform type checking on both sender and value: is the sender a “Timer”, that means, is it an instance of the class Timer? Or is the sender exactly the object called Timer_10Hz? Or is the value I just received a Vector of 8 doubles?
-	It can un/subscribe to/from a Block dynamically, at run-time, with no limitations.
-	Performs some internal processing using the values it receives from its input Blocks.
-	Fires its own event SendOutput(value) when it has finished. Therefore, all Blocks which have subscribed to it will have their own OnNewInput fired. The sender will be this very Block and the value will be, well, this very value.
