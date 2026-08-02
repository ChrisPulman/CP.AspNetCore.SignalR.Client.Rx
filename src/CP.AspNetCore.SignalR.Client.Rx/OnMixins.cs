// Copyright (c) 2019-2026 Chris Pulman and contributors. All rights reserved.
// Chris Pulman and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.AspNetCore.SignalR.Client.Rx.Reactive;
#else
namespace CP.AspNetCore.SignalR.Client.Rx;
#endif

/// <summary>On Mixins.</summary>
public static class OnMixins
{
    /// <summary>Provides observable handlers for hub method callbacks.</summary>
    /// <param name="connection">The hub connection.</param>
    extension(HubConnection connection)
    {
        /// <summary>Registers a handler that will be invoked when the hub method with the specified method name is invoked.</summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>An observable reactive void value.</returns>
        public IObservable<RxVoid> On(string methodName) =>
            Observable.CreateSafe<RxVoid>(observer =>
            {
                var handler = new Action(() => observer.OnNext(RxVoid.Default));
                return connection.On(methodName, handler);
            });

        /// <summary>Registers a handler that will be invoked when the hub method with the specified method name is invoked.</summary>
        /// <typeparam name="T">The type of the parameter of the method that this delegate encapsulates.</typeparam>
        /// <param name="method">The strongly typed hub method.</param>
        /// <returns>An Observable of T.</returns>
        public IObservable<T> On<T>(HubMethod<T> method) =>
            Observable.CreateSafe<T>(observer =>
            {
                var handler = new Action<T>(observer.OnNext);
                return connection.On(method.Name, handler);
            });

        /// <summary>Registers a handler that will be invoked when the hub method with the specified method name is invoked.</summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="method">The strongly typed hub method.</param>
        /// <returns>An Observable of T1, T2.</returns>
        public IObservable<(T1 t1, T2 t2)> On<T1, T2>(HubMethod<(T1 t1, T2 t2)> method) =>
            Observable.CreateSafe<(T1 t1, T2 t2)>(observer =>
            {
                var handler = new Action<T1, T2>((t1, t2) => observer.OnNext((t1, t2)));
                return connection.On(method.Name, handler);
            });

        /// <summary>Registers a handler that will be invoked when the hub method with the specified method name is invoked.</summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="T3">The type of the 3.</typeparam>
        /// <param name="method">The strongly typed hub method.</param>
        /// <returns>An Observable of T1, T2, T3.</returns>
        public IObservable<(T1 t1, T2 t2, T3 t3)> On<T1, T2, T3>(HubMethod<(T1 t1, T2 t2, T3 t3)> method) =>
            Observable.CreateSafe<(T1 t1, T2 t2, T3 t3)>(observer =>
            {
                var handler = new Action<T1, T2, T3>((t1, t2, t3) => observer.OnNext((t1, t2, t3)));
                return connection.On(method.Name, handler);
            });

        /// <summary>Registers a handler that will be invoked when the hub method with the specified method name is invoked.</summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="T3">The type of the 3.</typeparam>
        /// <typeparam name="T4">The type of the 4.</typeparam>
        /// <param name="method">The strongly typed hub method.</param>
        /// <returns>An Observable of T1, T2, T3, T4.</returns>
        public IObservable<(T1 t1, T2 t2, T3 t3, T4 t4)> On<T1, T2, T3, T4>(HubMethod<(T1 t1, T2 t2, T3 t3, T4 t4)> method) =>
            Observable.CreateSafe<(T1 t1, T2 t2, T3 t3, T4 t4)>(observer =>
            {
                var handler = new Action<T1, T2, T3, T4>((t1, t2, t3, t4) => observer.OnNext((t1, t2, t3, t4)));
                return connection.On(method.Name, handler);
            });

        /// <summary>Registers a handler that will be invoked when the hub method with the specified method name is invoked.</summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="T3">The type of the 3.</typeparam>
        /// <typeparam name="T4">The type of the 4.</typeparam>
        /// <typeparam name="T5">The type of the 5.</typeparam>
        /// <param name="method">The strongly typed hub method.</param>
        /// <returns>An Observable of T1, T2, T3, T4, T5.</returns>
        public IObservable<(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)> On<T1, T2, T3, T4, T5>(HubMethod<(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)> method) =>
            Observable.CreateSafe<(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)>(observer =>
            {
                var handler = new Action<T1, T2, T3, T4, T5>((t1, t2, t3, t4, t5) => observer.OnNext((t1, t2, t3, t4, t5)));
                return connection.On(method.Name, handler);
            });

        /// <summary>Registers a handler that will be invoked when the hub method with the specified method name is invoked.</summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="T3">The type of the 3.</typeparam>
        /// <typeparam name="T4">The type of the 4.</typeparam>
        /// <typeparam name="T5">The type of the 5.</typeparam>
        /// <typeparam name="T6">The type of the 6.</typeparam>
        /// <param name="method">The strongly typed hub method.</param>
        /// <returns>An Observable of T1, T2, T3, T4, T5, T6.</returns>
        public IObservable<(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)> On<T1, T2, T3, T4, T5, T6>(HubMethod<(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)> method) =>
            Observable.CreateSafe<(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)>(observer =>
            {
                var handler = new Action<T1, T2, T3, T4, T5, T6>((t1, t2, t3, t4, t5, t6) => observer.OnNext((t1, t2, t3, t4, t5, t6)));
                return connection.On(method.Name, handler);
            });

        /// <summary>Registers a handler that will be invoked when the hub method with the specified method name is invoked.</summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="T3">The type of the 3.</typeparam>
        /// <typeparam name="T4">The type of the 4.</typeparam>
        /// <typeparam name="T5">The type of the 5.</typeparam>
        /// <typeparam name="T6">The type of the 6.</typeparam>
        /// <typeparam name="T7">The type of the 7.</typeparam>
        /// <param name="method">The strongly typed hub method.</param>
        /// <returns>An Observable of T1, T2, T3, T4, T5, T6, T7.</returns>
        public IObservable<(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)> On<T1, T2, T3, T4, T5, T6, T7>(HubMethod<(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)> method) =>
            Observable.CreateSafe<(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)>(observer =>
            {
                var handler = new Action<T1, T2, T3, T4, T5, T6, T7>((t1, t2, t3, t4, t5, t6, t7) => observer.OnNext((t1, t2, t3, t4, t5, t6, t7)));
                return connection.On(method.Name, handler);
            });

        /// <summary>Registers a handler that will be invoked when the hub method with the specified method name is invoked.</summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="T3">The type of the 3.</typeparam>
        /// <typeparam name="T4">The type of the 4.</typeparam>
        /// <typeparam name="T5">The type of the 5.</typeparam>
        /// <typeparam name="T6">The type of the 6.</typeparam>
        /// <typeparam name="T7">The type of the 7.</typeparam>
        /// <typeparam name="T8">The type of the 8.</typeparam>
        /// <param name="method">The strongly typed hub method.</param>
        /// <returns>An Observable of T1, T2, T3, T4, T5, T6, T7, T8.</returns>
        public IObservable<(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)> On<T1, T2, T3, T4, T5, T6, T7, T8>(HubMethod<(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)> method) =>
            Observable.CreateSafe<(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)>(observer =>
            {
                var handler = new Action<T1, T2, T3, T4, T5, T6, T7, T8>((t1, t2, t3, t4, t5, t6, t7, t8) => observer.OnNext((t1, t2, t3, t4, t5, t6, t7, t8)));
                return connection.On(method.Name, handler);
            });
    }
}
