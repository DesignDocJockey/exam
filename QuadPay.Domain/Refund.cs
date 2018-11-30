using System;

namespace QuadPay.Domain {
    public class Refund {

        public Guid Id { get; }
        public string IdempotencyKey { get; private set; }
        public DateTime Date { get; }
        public decimal Amount { get; private set; }

        public Refund(string idempotencyKey, decimal amount) {
            IdempotencyKey = idempotencyKey;
            Amount = amount;
        }

    }
}