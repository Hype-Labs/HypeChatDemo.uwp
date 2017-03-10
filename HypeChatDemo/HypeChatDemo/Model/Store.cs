using System;
using System.Collections.Generic;
using HypeLabs;

namespace HypeChatDemo
{
    public class Store
    {
        private Instance _instance;

        //List of received messages
        List<string> _messages;

        public Store(Instance instance)
        {
            _instance = instance;
        }
        
        public Instance Instance
        {
            get
            {
                return _instance;
            }
        }

        public void Add(String message)
        {
            Messages.Add(message);           
        }

        public List<string> Messages
        {
            get
            {
                if (_messages == null)
                {
                    _messages = new List<string>();
                }
                return _messages;
            }
        }

    }
}
