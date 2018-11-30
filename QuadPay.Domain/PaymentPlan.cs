using System;
using System.Linq;
using System.Collections.Generic;

namespace QuadPay.Domain
{
    public static class Defaults
    {
        public const int InstallmentCounts = 4;
        public const int InstallmentIntervalDays = 14;
    }

    public class PaymentPlan
    {
        public Guid Id { get; private set; }
        public IList<Installment> Installments { get; private set; }
        public IList<Refund> Refunds { get; }
        public DateTime OriginationDate { get; }

        public decimal TotalAmountOwed { get; private set; }
        public int NumberOfInstallments { get; private set; }
        public int InstallmentIntervalDays { get; private set; }

        public PaymentPlan(decimal amount, int installmentCount = Defaults.InstallmentCounts, int installmentIntervalDays = Defaults.InstallmentIntervalDays)
        {
            if (amount <= 0.0m || amount > Decimal.MaxValue)
                throw new ArgumentException($"Invalid Parameter. {nameof(amount)}:{amount}");

            if (installmentCount <= 0)
                throw new ArgumentException($"Invalid Parameter. {nameof(installmentCount)}:{installmentCount}");

            if (installmentIntervalDays <= 0)
                throw new ArgumentException($"Invalid Parameter. {nameof(installmentIntervalDays)}:{installmentIntervalDays}");

            //TODO::check that the number of days of installments cannot be less than the number of installments
            if (installmentIntervalDays > installmentCount) { }

            TotalAmountOwed = amount;
            NumberOfInstallments = installmentCount;
            InstallmentIntervalDays = installmentIntervalDays;
            OriginationDate = DateTime.Now;
            Id = Guid.NewGuid();

            InitializeInstallments();
        }

        // Installments are paid in order by Date
        public Installment NextInstallment()
        {
            // TODO
            return new Installment();
        }

        public Installment FirstInstallment()
        {
            if (!Installments.Any())
                throw new ApplicationException($"No Installments for Payment Plan Id {Id.ToString()}");

            return Installments.OrderBy(i => i.Date).FirstOrDefault();
        }

        public decimal OustandingBalance()
        {
            var pendingPaymentsAmount = Installments.Where(i => i.IsPending || i.IsDefaulted).Sum(i => i.Amount);
            return pendingPaymentsAmount;
        }

        public decimal AmountPastDue(DateTime currentDate)
        {
            // TODO
            return 0;
        }

        public IList<Installment> PaidInstallments()
        {
            // TODO
            return new List<Installment>();
        }

        public IList<Installment> DefaultedInstallments()
        {
            // TODO
            return new List<Installment>();
        }

        public IList<Installment> PendingInstallments()
        {
            // TODO
            return new List<Installment>();
        }

        public decimal MaximumRefundAvailable()
        {
            // TODO
            return 0;
        }

        // We only accept payments matching the Installment Amount.
        public void MakePayment(decimal amount, Guid installmentId) 
        {
            if(!Installments.Any(i => i.Id == installmentId)) {
                throw new ArgumentException($"No Installment Found for Provided installmentId: {installmentId}", nameof(installmentId));
            }

            var installment = Installments.Where(i => i.Id == installmentId).FirstOrDefault();

            if (installment.Amount != amount) {
                throw new ArgumentException($"Payment amount must match installment amount.", nameof(amount));
            }

            //we call payment domain logic here, ie. service call or writing to an event stream
            var paymentReferenceId = Guid.NewGuid(); //in this insstance, i'm just setting some guid to be the payment reference id, but a real-world implementation would
            //call another service i.e 3rd party payment API
            installment.SetPaid(paymentReferenceId.ToString());
        }

        // Returns: Amount to refund via PaymentProvider
        public decimal ApplyRefund(Refund refund)
        {
            var paidAmounts = Installments.Where(i => i.IsPaid).Sum(j => j.Amount);

            //call Payment Provider to retrieve refund
            //refund.Amount


            // TODO
            return 0;
        }

        // First Installment always occurs on PaymentPlan creation date
        private void InitializeInstallments()
        {
            Installments = new List<Installment>(NumberOfInstallments);
            var paymentAmountPerInstallment = TotalAmountOwed / NumberOfInstallments;

            //add first payment
            var initialPayment = new Installment(paymentAmountPerInstallment, OriginationDate);
            Installments.Add(initialPayment);

            var paymentDate = OriginationDate;
            //other installments
            for (var i = 1; i < NumberOfInstallments; i++) 
            {
                paymentDate = paymentDate.AddDays(InstallmentIntervalDays);
                var installmentPayment = new Installment(paymentAmountPerInstallment, paymentDate);
                Installments.Add(installmentPayment);
            }

        }
    }
}