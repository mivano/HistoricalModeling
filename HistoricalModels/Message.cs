using System;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TMS.DataContracts.HistoricalModels
{
    public readonly struct Message
        : IEquatable<Message>
    {
        private readonly MessageContent _content;
        private readonly string _signature;

        public Message(string propertyName, object? value)
            : this(propertyName, new[] { string.Empty }, value)
        {
        }

        public Message(string propertyName, Message predecessor, object? value)
            : this(propertyName, new[] { predecessor.GetSignature() }, value)
        {
        }

        public Message(string propertyName, string predecessor, object? value)
            : this(propertyName, new[] { predecessor }, value)
        {
        }

        public Message(string propertyName, Message[] predecessors, object? value)
        {
            var parentSignatures = predecessors.Select(x => x.GetSignature()).ToArray();
            _content = new MessageContent(propertyName, parentSignatures, value);
            _signature = CalculateSignature(_content);
        }

        public Message(string propertyName, string[] predecessors, object? value)
        {
            _content = new MessageContent(propertyName, predecessors, value);
            _signature = CalculateSignature(_content);
        }
       

        public string GetSignature() => _signature;

        public override string ToString() => $"Property: {_content.PropertyName}; Value: {_content.Value}; Signature: {_signature};";

        private static string CalculateSignature(MessageContent content)
        {
            var json = JsonSerializer.Serialize(content);
            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
            var stringBuilder = new StringBuilder();

            foreach (var b in bytes)
            {
                stringBuilder.Append(b.ToString("x2"));
            }

            return stringBuilder.ToString();
        }

        private readonly struct MessageContent
        {
            public ImmutableSortedSet<string> Predecessors { get; }
            public string PropertyName { get; }
            public object? Value { get; }

            public MessageContent(string propertyName, string[] parentSignatures, object? value)
            {
                Predecessors = ImmutableSortedSet.Create(parentSignatures);
                PropertyName = propertyName;
                Value = value;
            }
        }

        public bool Equals(Message other)
        {
            return _signature.Equals(other._signature);
        }

        public override bool Equals(object obj)
        {
            return obj is Message other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return _signature.GetHashCode();
            }
        }
    }
}