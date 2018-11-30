using System;

namespace QuadPay.Domain
{
    public class Installment
    {
        public Guid Id { get; private set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        private InstallmentStatus InstallmentStatus;
        public string PaymentReference { get; private set; }

        // Date this Installment was marked 'Paid'
        public DateTime SettlementDate { get; private set; }

        //Provide Default Constructor
        public Installment() { }

        public Installment(decimal amountDue, DateTime dueDate)
        {
            Id = Guid.NewGuid();
            Amount = amountDue;
            Date = dueDate;
        }

        public bool IsPaid 
        { 
            get 
            {
                return (InstallmentStatus == InstallmentStatus.Paid) ? true : false;
            }
        }

        public bool IsDefaulted 
        {
            get
            {
                return (InstallmentStatus == InstallmentStatus.Defaulted) ? true : false;
            }
        }

        public bool IsPending 
        {
            get
            {
                return (InstallmentStatus == InstallmentStatus.Pending) ? true : false;
            }
        }

        public void SetPaid(string paymentReference)
        {    
            PaymentReference = paymentReference;
            InstallmentStatus = InstallmentStatus.Paid;
            SettlementDate = DateTime.Now;
        }
    }

    public enum InstallmentStatus {
        Pending = 0, // Not yet paid
        Paid = 1, // Can be either paid with a charge, or covered by a refund
        Defaulted = 2 // Charge failed
    }
}