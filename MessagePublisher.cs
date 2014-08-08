using System;
using System.Collections.Generic;

namespace ClassLibrary1
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly IDictionary<Type, IDictionary<Type, WeakReference<object>>> subscriptionList;
        private readonly object readerWriterLock;

        public MessagePublisher()
        {
            this.subscriptionList = new Dictionary<Type, IDictionary<Type, WeakReference<object>>>();
            this.readerWriterLock = new object();
        }

        public void Subscribe<TMessage, TSubcriber>(TSubcriber messageReciever) where TSubcriber : IMessageRecievier<TMessage>
        {
            IDictionary<Type, WeakReference<object>> subscribers;
            var messageType = typeof(TMessage);
            var subscriberType = typeof(TSubcriber);

            if (!this.subscriptionList.TryGetValue(messageType, out subscribers))
            {
                lock (readerWriterLock)
                {
                    CheckForMessages<TMessage, TSubcriber>(messageReciever, subscribers, messageType, subscriberType); 
                }
            }
            else
            {
                lock (readerWriterLock)
                {
                    CheckForsubscribers<TMessage, TSubcriber>(messageReciever, subscribers, subscriberType);
                }
            }
        }

        private void CheckForMessages<TMessage, TSubcriber>(TSubcriber messageReciever, IDictionary<Type, WeakReference<object>> subscribers, Type messageType, Type subscriberType) where TSubcriber : IMessageRecievier<TMessage>
        {
            if (!this.subscriptionList.TryGetValue(messageType, out subscribers))
            {
                this.subscriptionList.Add(messageType, new Dictionary<Type, WeakReference<object>>() 
                        {
                            { subscriberType, new WeakReference<object>(messageReciever) }
                        });
            }
            else
            {
                CheckForsubscribers<TMessage, TSubcriber>(messageReciever, subscribers, subscriberType);
            }
        }

        private void CheckForsubscribers<TMessage, TSubcriber>(TSubcriber messageReciever, IDictionary<Type, WeakReference<object>> subscribers, Type subscriberType) where TSubcriber : IMessageRecievier<TMessage>
        {
            WeakReference<object> subscriber;
            if (!subscribers.TryGetValue(subscriberType, out subscriber))
            {
                subscribers.Add(subscriberType, new WeakReference<object>(messageReciever));
            }
            else
            {
                object target;
                if (!subscriber.TryGetTarget(out target))
                {
                    subscriber.SetTarget(messageReciever);
                }
                else
                {
                    throw new Exception("The given type is already in the subscription list.");
                }
            }
        }

        public void Broadcast<TMessage>(TMessage message)
        {
            IDictionary<Type, WeakReference<object>> subscribers;
            var messageType = typeof(TMessage);

            if (this.subscriptionList.TryGetValue(messageType, out subscribers))
            {
                lock (readerWriterLock)
                {
                    if (this.subscriptionList.TryGetValue(messageType, out subscribers))
                    {
                        foreach (var subscriber in subscribers)
                        {
                            object target;
                            if (subscriber.Value.TryGetTarget(out target))
                            {
                                var reciever = target as IMessageRecievier<TMessage>;
                                if(reciever!=null)
                                {
                                    reciever.Recieve(message);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}