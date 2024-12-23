# Branching Strategies - Clean Domain

This repository contains examples of refactoring an ASP.NET Core MVC Controller to improve maintainability by reducing deeply nested code and simplifying branching logic.

This guide is an extension of the techniques discussed in the article: "[.NET Strategies to Bypass Branching Madness: Tactics for Lean Domain Logic in C#](https://medium.com/gitconnected/net-strategies-to-bypass-branching-madness-tactics-for-lean-domain-logic-in-c-fbb6bc51f48b)", in which middlewares, Aspect-Oriented Programming techniques, and model validation are leveraged to demonstrate how a cleaner, more maintainable Web API codebase can be achieved in C#.

## Contents

- [Introduction](#introduction)
- [Embracing Middlewares](#embracing-middlewares)
- [Aspect-Oriented Programming (AOP)](#aspect-oriented-programming-aop)
- [Enriching Models with Data Annotations](#enriching-models-with-data-annotations)
- [Conclusion](#conclusion)

## Introduction

Deeply nested code is a common anti-pattern that complicates code maintainability. This repository demonstrates how to refactor an ASP.NET Core MVC Controller by segregating concerns such as authentication, authorization, and validation from the action methods into more maintainable structures.

## Embracing Middlewares

Middlewares are utilized to create a request processing pipeline in ASP.NET Core, allowing concerns like logging and error handling to be abstracted from action methods. See how this can result in more manageable code by examining the provided examples in the `CleanDomain1Middlewares` directory.

## Aspect-Oriented Programming (AOP)

AOP techniques enable a clean separation of concerns by extracting cross-cutting concerns into separate aspects. This allows action methods to focus solely on business logic. Explore how attributes can be used for declarative authorization in examples located in the `CleanDomain2Aspects` directory.

## Enriching Models with Data Annotations

In the `CleanDomain3DataAnnotations` directory, you'll find examples of how data annotations can be used for model validation. By applying data annotations to model properties, manual validation logic within action methods can be significantly reduced or eliminated.

## Conclusion

The examples in this repository illustrate how ASP.NET Core code can be refactored using middleware, AOP, and data annotations to adhere to the principles of Separation of Concerns ([SoC](https://cln.co/SoC)), Dependency Inversion Principle ([DIP](https://cln.co/DIP)), and Single Point Of Truth ([SPOT](https://cln.co/SPOT)). These practices enhance maintainability, readability, and scalability of Web API code in ASP.NET Core.

For the complete guide and deeper insights into these patterns, refer to the article "[.NET Strategies to Bypass Branching Madness: Tactics for Lean Domain Logic in C#](https://medium.com/gitconnected/net-strategies-to-bypass-branching-madness-tactics-for-lean-domain-logic-in-c-fbb6bc51f48b)."

## Getting Started

To get started with these examples:

1. Clone this repository using `git clone URL`.
2. Review the examples in each directory to understand how refactoring is done.
3. Integrate these concepts into your existing projects to enhance code maintainability and reduce complexity.


Happy coding, and remember to keep it clean, maintainable, and scalable!