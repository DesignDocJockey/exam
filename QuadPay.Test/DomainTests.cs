using System;
using Xunit;
using QuadPay.Domain;

namespace QuadPay.Test
{
    public class DomainTests
    {
        [Theory]
        [InlineData(-100, 4, 2)]
        [InlineData(123.23, 0, 2)]

        // TODO What other situations should we be testing?
        [InlineData(0.00, 4, 14)]
        [InlineData(500.00, 4, 0)]
        public void ShouldThrowExceptionForInvalidParameters(decimal amount, int installmentCount, int installmentIntervalDays)
        {

            Assert.Throws<ArgumentException>(() => {
                var paymentPlan = new PaymentPlan(amount, installmentCount, installmentIntervalDays);
            });
        }

        [Theory]
        [InlineData(1000, 4, 2)]
        [InlineData(123.23, 2, 2)]
        // TODO What other situations should we be testing?
        public void ShouldCreateCorrectNumberOfInstallments(decimal amount, int installmentCount, int installmentIntervalDays)
        {
            var paymentPlan = new PaymentPlan(amount, installmentCount, installmentIntervalDays);
            Assert.Equal(installmentCount, paymentPlan.Installments.Count);
        }

        [Fact]
        public void ShouldReturnCorrectAmountToRefundAgainstPaidInstallments() {
            var paymentPlan = new PaymentPlan(100, 4);
            var firstInstallment = paymentPlan.FirstInstallment();
            paymentPlan.MakePayment(25, firstInstallment.Id);
            var cashRefundAmount = paymentPlan.ApplyRefund(new Refund(Guid.NewGuid().ToString(), 100));
            Assert.Equal(25, cashRefundAmount);
            Assert.Equal(0, paymentPlan.OustandingBalance());
        }

        [Fact]
        public void ShouldReturnCorrectOutstandingBalance() {
            var paymentPlan = new PaymentPlan(100, 4);
            var firstInstallment = paymentPlan.FirstInstallment();
            paymentPlan.MakePayment(25, firstInstallment.Id);
            var secondInstallment = paymentPlan.NextInstallment();
            paymentPlan.MakePayment(25, secondInstallment.Id);
            Assert.Equal(50, paymentPlan.OustandingBalance());
        }

        /*
            TODO
            Increase domain test coverage
         */
    }
}
