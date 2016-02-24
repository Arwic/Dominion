// Dominion - Copyright (C) Timothy Ings
// Controller.cs
// This file defines classes that defines base classes for game controllers

namespace Dominion.Server.Controllers
{
    public abstract class Controller
    {
        public ControllerManager Controllers { get; }

        public Controller(ControllerManager manager)
        {
            Controllers = manager;
        }

        public virtual void ProcessTurn()
        {

        }
    }
}
