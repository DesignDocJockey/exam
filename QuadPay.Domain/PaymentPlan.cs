using System;
using System.Collections.Generic;

namespace QuadPay.Domain
{
    public static class Defaults {
        public const int InstallmentCounts = 4;
        public const int InstallmentIntervalDays = 14;
    }

    public class PaymentPlan
    {
        public Guid Id { get; }
        public IList<Installment> Installments { get; }
        public IList<Refund> Refunds { get; }
        public DateTime OriginationDate { get; }

        public decimal TotalAmountOwed { get; private set; }
        public int NumberOfInstallments { get; private set; }
        public int InstallmentIntervalDays { get; private set; }

        public PaymentPlan(decimal amount, int installmentCount = Defaults.InstallmentCounts, int installmentIntervalDays = Defaults.InstallmentIntervalDays)
        {
            if (amount <= 0.0m || amount > Decimal.MaxValue)
                throw new ArgumentException($"Invalid Parameter. {nameof(amount)}:{amount}");

            if (installmentCount <= 0 || installmentCount != Defaults.InstallmentCounts)
                throw new ArgumentException($"Invalid Parameter. {nameof(installmentCount)}:{installmentCount}");

            if (installmentIntervalDays <= 0 || installmentIntervalDays != Defaults.InstallmentIntervalDays)
                throw new ArgumentException($"Invalid Parameter. {nameof(installmentIntervalDays)}:{installmentIntervalDays}");

            //TODO::check that the number of days of installments cannot be less than the number of installments
            if (installmentIntervalDays > installmentCount) { }

            TotalAmountOwed = amount;
            NumberOfInstallments = installmentCount;
            InstallmentIntervalDays = installmentIntervalDays;
            OriginationDate = DateTime.Now;

            // TODO
            InitializeInstallments();
        }

        // Installments are paid in order by Date
        public Installment NextInstallment() {
            // TODO
            return new Installment();
        }

        public Installment FirstInstallment() {
            // TODO
            return new Installment();
        }

        public decimal OustandingBalance() {
            // TODO
            return 0;
        }

        public decimal AmountPastDue(DateTime currentDate) {
            // TODO
            return 0;
        }

        public IList<Installment> PaidInstallments() {
            // TODO
            return new List<Installment>();
        }

        public IList<Installment> DefaultedInstallments() {
            // TODO
            return new List<Installment>();
        }

        public IList<Installment> PendingInstallments() {
            // TODO
            return new List<Installment>();
        }

        public decimal MaximumRefundAvailable() {
            // TODO
            return 0;
        }

        // We only accept payments matching the Installment Amount.
        public void MakePayment(decimal amount, Guid installmentId) {

        }

        // Returns: Amount to refund via PaymentProvider
        public decimal ApplyRefund(Refund refund) {
            // TODO
            return 0;
        }

        // First Installment always occurs on PaymentPlan creation date
        private void InitializeInstallments()
        {
            // TODO

            var paymentAmountPerInstallment = TotalAmountOwed / NumberOfInstallments;

            for (var i = 0; i < NumberOfInstallments; i++) {
                var installmentPayment = new Installment(paymentAmountPerInstallment,
                    OriginationDate.AddDays(InstallmentIntervalDays));
            }

        }
    }
}