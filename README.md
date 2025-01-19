# GlueBox - Hanne Bogaerts

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Architecture](#architecture)
- [Logging](#logging)
- [Advanced Features](#advanced-features)
- [DemoApplications](#demoapplication)
- [Testing](#testing)

---

## Overview

Welcome to **GlueBox**!

GlueBox is a lightweight, flexible Dependency Injection container for .NET. It simplifies dependency management, keeping services stuck together in all the right ways.

Key Features:
- Lightweight and simple to use
- Customizable logging with Serilog integration
- Advanced features like attribute-based cross-cutting concerns and command handling

---

## Getting Started

### Installation

1. Add the GlueBox source files to your .NET project.
2. Ensure all services registered in GlueBox have parameterless constructors.

---

## Usage

### 1. Initialize the GlueBox container

Start by creating an instance of the GlueBox container:

```csharp
var glueBox = new GlueBox();
```

### 2. Register services

GlueBox supports both singleton and transient service registrations.

#### Register as Singleton
```csharp
glueBox.StickSingleton<IService, Service>();
glueBox.StickSingleton<Service>();
```

#### Register as Transient
```csharp
glueBox.StickTransient<IService, Service>();
glueBox.StickTransient<Service>();
```

### 3. Register controllers and complete adhesion (optional)

This method scans the assembly for all types implementing IGlueController and registers them as singletons.

```csharp
glueBox.CompleteAdhesion();
```

### 4. Resolve services

Resolve instances of registered services:

```csharp
var service = glueBox.Resolve<IService>();
```

### 5. Customize logging (optional)

#### Set Log Level
The default log level is `Information`. You can customize it to one of the following: `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`.

```csharp
GlueBox.SetLogLevel("Debug");
```

#### Set Log Output
The default log output is `Console`. You can change it to `File` and optionally add a custom output:

```csharp
GlueBox.SetLogOutput("File");
GlueBox.SetLogOutput("File", "C:/Logs/GlueBoxLog.txt");
```

---

## Architecture

GlueBox's architecture is designed for simplicity and flexibility:

- **GlueBox**: The main entry point for registering and resolving services.
- **AdhesionManager**: Manages dependencies and ensures smooth resolution of service graphs.
- **AdhesionRegistry**: Stores mappings between interfaces and their implementations.
- **AdhesionResolver**: Handles dynamic instantiation and dependency injection.
- **CycleBreaker**: Prevents circular dependencies by tracking service resolution.
- **GlueBinding**: Represents a binding between a service and a lifetime.
- **IGlueController**: Interface for controllers that can be registered with GlueBox.

---

## Logging

GlueBox leverages **Serilog** for robust logging capabilities. You can configure the log level and output to suit your needs, ensuring seamless debugging and monitoring of your application.

### Key Features:
- Multiple log levels for precise control over output verbosity
- Support for both console and file outputs
- Customizable log destinations

---

## Advanced Features

### Cross-Cutting Concerns with Attributes and Interceptors
GlueBox supports cross-cutting concerns such as logging and performance measurement using attributes and interceptors. The attributes can be applied to methods or classes to add additional functionality without modifying the original code.

**Note**: Methods that use these attributes must be marked as `virtual`.

```csharp
[Attribute]
public class MyClass
{
    public virtual void MyMethod()
    {
        // Method logic here
    }
}
```

```csharp
[Attribute]
public virtual void MyMethod()
{
    // Method logic here
}
```

#### GlueTimerAttribute

Use the `GlueTimerAttribute` to measure the execution time of methods or all methods within a class. The timing information is logged via Serilog.

**Note**: Methods that use this attribute must be marked as `virtual`.

#### StickTraceAttribute

Use the `StickTraceAttribute` to log method entry and exit points, providing detailed tracing of method calls within a class.

**Note**: Methods that use this attribute must be marked as `virtual`.


### Command Handling with Attributes
GlueBox supports user command handling in the console via the GlueInputAttribute. This allows methods to be mapped to specific commands and invoked when the command is triggered.

```csharp
[GlueInput("hello")]
public void SayHello()
{
    Console.WriteLine("Hello!");
}
```

The command is registered and can be invoked using GlueBox's `CommandAdhesionMap`.

```csharp
var glueBox = new GlueBox();
var action = glueBox.CommandAdhesionMap["hello"];
action(); 
```
**Note**: Methods that use this attribute must be parameterless.

---
Happy coding with GlueBox! 🚀