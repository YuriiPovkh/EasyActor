﻿using Castle.DynamicProxy;
using System;
using EasyActor.TaskHelper;
using System.Reflection;
using System.Diagnostics;
using EasyActor.Proxy;
using EasyActor.Factories;

namespace EasyActor
{
    internal class ActorCompleteLifeCycleInterceptor:  InterfaceInterceptor<IActorCompleteLifeCycle>, IInterceptor
    {
        private static readonly MethodInfo _Abort = _Type.GetMethod("Abort", BindingFlags.Instance | BindingFlags.Public);

        private readonly IAbortableTaskQueue _Queue;
        private readonly IAsyncDisposable _IAsyncDisposable;

        public ActorCompleteLifeCycleInterceptor(IAbortableTaskQueue iqueue, IAsyncDisposable iAsyncDisposable)
        {
            _Queue = iqueue;
            _IAsyncDisposable = iAsyncDisposable;
        }

        [DebuggerNonUserCode]
        protected override object InterceptClassMethod(IInvocation invocation)
        {
            if (invocation.Method != _Abort)
                throw new ArgumentOutOfRangeException();

            return _Queue.Abort(() =>
            {
                ActorFactoryBase.Clean(invocation.Proxy);
                return (_IAsyncDisposable != null) ? _IAsyncDisposable.DisposeAsync() : TaskBuilder.Completed;
            });
        }
    }
}
