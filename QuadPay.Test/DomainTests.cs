using System;
using Xunit;
using QuadPay.Domain;

namespace QuadPay.Test
{
    public class DomainTests
    {
        [Theory]
        [InlineData(-100, 4, 2)]
        [InlineData(123.23, 0, 2)] // What other situations should we be testing?

        [InlineData(0.00, 4, 14)]    //test when amount is $0.00, the payment plan cannot be created
        [InlineData(500.00, -5, 0)]  //test when the installment count  cannot be a negative value
        [InlineData(500.00, 4, 0)]   //test when the number of days between installment payments cannot be 0 
        [InlineData(500.00, 4, -10)] //test when the number of days between installment payments cannot be a negative value 
        public void ShouldThrowExceptionForInvalidParameters(decimal amount, int installmentCount, int installmentIntervalDays)
        {
            Assert.Throws<ArgumentException>(() => {
                var paymentPlan = new PaymentPlan(amount, installmentCount, installmentIntervalDays);
            });
        }

        [Theory]
        //[InlineData(1000, 4, 2)]
        //[InlineData(123.23, 2, 2)]   // TODO What other situations should we be testing?

        [InlineData(10000, 1, 1)] //test that a payment plan can be a full payment (Perhaps this is in the domain logic?)
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

        //Test the total installments owed shoudl add up to the original amount;
        //Test for an invalid installment guid when calling MakePayment
    }
}
