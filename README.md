# Myre

Myre is a set of libraries for building games in C# using XNA. Myre has been used extensively over the years as the basis for many of my own projects.

## Myre.Debugging.UI

Places a console overlay in game. Pressing the console key (default on a UK keyboard is the ` key) shows a console. Commands can be bound to the console and then called at any time. Variables can be bound and can either be set or pritned at any time. The console comes with autocompletion for simple statements.

## Myre.Debugging

An assorted set of helpers for debugging. Contains the `CommandEngine` which powers the console. Has helpers for tracking a value over time.

## Myre.Entities

A scene graph system.
  - Entities (properties and behaviours)
  - Behaviour (object attached to entities)
  - Property (data store attached to an entity)
  - BehaviourManager (enumerates all behaviours of a given type which are currently in the scene every frame)
  - Service (A service, not associated with any particular entity, which runs every frame)
  
## Myre.Graphics

A library for building graphical rendering systems. Contains abstracts for a pipeline of actions, each actions may retrieve resources already in the pipeline and update or add new resources. Also contains a deferred light prepass renderer which is built upon this pipeline system.

## Myre.StateManagement

A simple statemanagement system intended for controlling screens in a game. Supports a stack of screens and runs fade in/out transitions when screen are changed.

## Myre.UI

A library of UI components which render using XNA. Has flexible base classes for controls and input gestures but not very many controls actually implemented - mostly exists to support Myre.Debugging.UI which is built entirely upon this.

## Myre

Base library which all other Myre libraries build upon. Contains general utility types, datastructures and convenient extension methods.
