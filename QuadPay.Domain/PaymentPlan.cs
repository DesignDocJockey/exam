using System;
using System.Linq;
using System.Collections.Generic;

namespace QuadPay.Domain
{
    public static class DefaultValues
    {
        public const int InstallmentCounts = 4;
        public const int InstallmentIntervalDays = 14;
    }

    public class PaymentPlan
    {
        public Guid Id { get; private set; }
        public IList<Installment> Installments { get; private set; }
        public IList<Refund> Refunds { get; private set; }
        public DateTime OriginationDate { get; }

        public decimal TotalAmountOwed { get; private set; }
        public int NumberOfInstallments { get; private set; }
        public int InstallmentIntervalDays { get; private set; }

        public PaymentPlan(decimal amount, int installmentCount = DefaultValues.InstallmentCounts, int installmentIntervalDays = DefaultValues.InstallmentIntervalDays)
        {
            if (amount <= 0.0m || amount > Decimal.MaxValue)
                throw new ArgumentException($"Invalid Parameter. {nameof(amount)}:{amount}");

            if (installmentCount <= 0)
                throw new ArgumentException($"Invalid Parameter. {nameof(installmentCount)}:{installmentCount}");

            if (installmentIntervalDays <= 0)
                throw new ArgumentException($"Invalid Parameter. {nameof(installmentIntervalDays)}:{installmentIntervalDays}");

            //TODO::check that the number of days of installments cannot be less than the number of installments
            if (installmentIntervalDays > installmentCount) { }

            Id = Guid.NewGuid();
            TotalAmountOwed = amount;
            NumberOfInstallments = installmentCount;
            InstallmentIntervalDays = installmentIntervalDays;
            OriginationDate = DateTime.Now;
            InitializeInstallments();
        }

        // Installments are paid in order by Date
        public Installment NextInstallment()
        {
            var next = Installments.Where(i => i.IsPending)
                           .OrderBy(i => i.Date)
                           .FirstOrDefault();

            return next;
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
            return Installments.Where(i => currentDate.CompareTo(i.Date) == 1 && i.IsPending == true)
                                .Select(i => i.Amount)
                                .Sum();
        }

        public IList<Installment> PaidInstallments()
        {
            return Installments.Where(i => i.IsPaid).ToList<Installment>();
        }

        public IList<Installment> DefaultedInstallments()
        {
            return Installments.Where(i => i.IsDefaulted).ToList<Installment>();
        }

        public IList<Installment> PendingInstallments()
        {
            return Installments.Where(i => i.IsPending).ToList<Installment>();
        }

        public decimal MaximumRefundAvailable()
        {
            //Not really clear on the domain logic for refunds
            var maxRefundAvailable = 0.0m;
            if (Refunds != null) {
                maxRefundAvailable = Refunds.Sum(i => i.Amount);
            }

            return maxRefundAvailable;
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
            var paymentReferenceId = Guid.NewGuid(); //in this instance, i'm just setting some guid to be the payment reference id, 
            //but an actual implementation could call another service i.e 3rd party payment API
            installment.SetPaid(paymentReferenceId.ToString());
        }

        // Returns: Amount to refund via PaymentProvider
        public decimal ApplyRefund(Refund refund)
        {
            var refundedAmount = Installments.Where(i => i.IsPaid).Sum(j => j.Amount);

            if (Refunds == null) {
                Refunds = new List<Refund>();
            }
            Refunds.Add(refund);

            var refundBalance = refund.Amount;
            foreach (var installment in Installments)
            {
                if(refundBalance >= installment.Amount)
                    MakePayment(installment.Amount, installment.Id);

                refundBalance = refundBalance - installment.Amount;
            }

            return refundedAmount;
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