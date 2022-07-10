﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyProxy.Objects.Delegates
{
    public delegate void BeforeMethodCall(BeforeMethodCallArgs args);

    public class BeforeMethodCallArgs 
    {
        public object? Sender { get; }
        public MethodInfo? Method { get; }

        public object?[]? Arguments { get; }
        
        public BeforeMethodCallArgs(object sender, string name, object[] args)
        {
            Sender = sender;
            Method = sender.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, args.Select(s => s.GetType()).ToArray());
            Arguments = args;
        }
    }
}
