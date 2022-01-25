﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Payments;
using Resto.Front.Api.Extensions;

namespace Resto.Front.Api.SamplePaymentPlugin
{
    internal sealed class PaymentEditorTester : IDisposable
    {
        private Window window;
        private ListBox buttons;

        public PaymentEditorTester()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        private void EntryPoint()
        {
            buttons = new ListBox();
            window = new Window
            {
                Content = buttons,
                Width = 310,
                Height = 870,
                Topmost = true,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                ResizeMode = ResizeMode.CanResize,
                ShowInTaskbar = true,
                VerticalContentAlignment = VerticalAlignment.Center,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.SingleBorderWindow,
            };

            AddButton("Add cash payment", AddCashPayment);
            AddButton("Add card payment", AddCardPayment);

            AddSeparator();

            AddButton("Add cash preliminary payment", AddCashPreliminaryPayment);
            AddButton("Add card preliminary payment", AddCardPreliminaryPayment);
            AddButton("Add credit preliminary payment", AddCreditPreliminaryPayment);
            AddButton("Delete preliminary payment", DeletePreliminaryPayment);
            AddButton("Add cash preliminary payment with discount", AddCashPreliminaryPaymentWithDiscount);
            AddButton("Delete preliminary payment with discount", DeletePreliminaryPaymentWithDiscount);

            AddSeparator();

            AddButton("Add cash external not processed payment", AddCashExternalNotProcessedPayment);
            AddButton("Add card external not processed payment", AddCardExternalNotProcessedPayment);
            AddButton("Add plugin external not processed payment", AddPluginExternalNotProcessedPayment);
            AddButton("Add cash external processed payment", AddCashExternalProcessedPayment);
            AddButton("Add card external processed payment", AddCardExternalProcessedPayment);

            AddSeparator();

            AddButton("Add cash external processed prepay", AddCashExternalProcessedPrepay);
            AddButton("Add card external processed prepay", AddCardExternalProcessedPrepay);
            AddButton("Add plugin external not processed prepay", AddPluginExternalNotProcessedPrepay);

            AddSeparator();

            AddButton("Add cash processed donation", AddCashProcessedDonation);
            AddButton("Add card processed donation", AddCardProcessedDonation);
            AddButton("Add plugin processed donation", AddPluginProcessedDonation);
            AddButton("Add plugin not processed donation", AddPluginNotProcessedDonation);
            AddButton("Delete donation", DeleteDonation);

            AddSeparator();

            AddButton("Add external fiscalized payment", AddExternalFiscalizedPayment);
            AddButton("Add external fiscalized prepay", AddExternalFiscalizedPrepay);
            AddButton("Delete external fiscalized payment", DeleteExternalFiscalizedPayment);

            AddSeparator();

            AddButton("Pay order on cash", PayOrderOnCashAndPayOutOnUser);
            AddButton("Pay order on card", PayOrderOnCardAndPayOutOnUser);
            AddButton("Pay order with existing payments", PayOrderWithExistingPayments);

            AddSeparator();

            AddButton("Open cafe session", OpenCafeSession);
            AddButton("Close cafe session", CloseCafeSession);

            AddSeparator();

            AddButton("Create order and process prepayments", CreateOrderAndProcessPrepayments);
            AddButton("Add and process preliminary payment", AddAndProcessPreliminaryPayment);

            window.ShowDialog();
        }

        private void AddSeparator()
        {
            buttons.Items.Add(new Separator { Margin = new Thickness(0, 4, 0, 4) });
        }

        private void AddButton(string text, Action action)
        {
            var button = new Button { Content = text, Margin = new Thickness(2) };
            button.Click += (s, e) =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    var message = $"{ex.GetType()}{Environment.NewLine}Cannot {text} :-({Environment.NewLine}Message: {ex.Message}{Environment.NewLine}{ex.StackTrace}";
                    MessageBox.Show(message, GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            buttons.Items.Add(button);
        }

        #region Add payment

        /// <summary>
        /// Добавление обычного платежа наличными.
        /// </summary>
        private void AddCashPayment()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Cash);
            PluginContext.Operations.AddPaymentItem(50, null, paymentType, order, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Добавление обычного платежа картой.
        /// </summary>
        private void AddCardPayment()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            PluginContext.Operations.AddPaymentItem(50, additionalData, paymentType, order, PluginContext.Operations.GetCredentials());
        }

        #endregion Add payment

        #region Add or delete preliminary payment

        /// <summary>
        /// Добавление отложенного платежа наличными.
        /// </summary>
        private void AddCashPreliminaryPayment()
        {
            var deliveryOrder = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Cash);
            PluginContext.Operations.AddPreliminaryPaymentItem(100, null, paymentType, deliveryOrder, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Добавление отложенного платежа картой.
        /// </summary>
        private void AddCardPreliminaryPayment()
        {
            var deliveryOrder = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            PluginContext.Operations.AddPreliminaryPaymentItem(150, additionalData, paymentType, deliveryOrder, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Добавление отложенного платежа в кредит на контрагента.
        /// </summary>
        private void AddCreditPreliminaryPayment()
        {
            var deliveryOrder = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Credit && x.Name.ToUpper().StartsWith("БЕЗН"));
            var user = PluginContext.Operations.GetUsers().Last(u => u.Name.ToUpper().StartsWith("BBB"));
            var additionalData = new CreditPaymentItemAdditionalData { CounteragentUserId = user.Id };
            PluginContext.Operations.AddPreliminaryPaymentItem(200, additionalData, paymentType, deliveryOrder, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Удаление отложенного платежа.
        /// </summary>
        private void DeletePreliminaryPayment()
        {
            var deliveryOrder = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentItem = deliveryOrder.Payments.First(i => i.IsPreliminary);
            PluginContext.Operations.DeletePreliminaryPaymentItem(paymentItem, deliveryOrder, PluginContext.Operations.GetCredentials());
        }

        /// <summary>
        /// Добавление отложенного платежа наличными со скидкой из типа оплаты.
        /// </summary>
        private void AddCashPreliminaryPaymentWithDiscount()
        {
            var deliveryOrder = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Cash && x.DiscountType != null);
            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.AddDiscount(paymentType.DiscountType, deliveryOrder);
            editSession.AddPreliminaryPaymentItem(100, null, paymentType, deliveryOrder);
            PluginContext.Operations.SubmitChanges(PluginContext.Operations.GetCredentials(), editSession);
        }

        /// <summary>
        /// Удаление отложенного платежа со скидкой из типа оплаты.
        /// </summary>
        private void DeletePreliminaryPaymentWithDiscount()
        {
            var deliveryOrder = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentItem = deliveryOrder.Payments.First(i => i.IsPreliminary && i.Type.DiscountType != null);
            var discountItem = deliveryOrder.Discounts.Last(d => d.DiscountType.Equals(paymentItem.Type.DiscountType));
            var editSession = PluginContext.Operations.CreateEditSession();
            editSession.DeleteDiscount(discountItem, deliveryOrder);
            editSession.DeletePreliminaryPaymentItem(paymentItem, deliveryOrder);
            PluginContext.Operations.SubmitChanges(PluginContext.Operations.GetCredentials(), editSession);
        }

        #endregion Add or delete preliminary payment

        #region Add or delete external payment

        /// <summary>
        /// Добавление внешнего непроведенного платежа наличными.
        /// </summary>
        private void AddCashExternalNotProcessedPayment()
        {
            const bool isProcessed = false;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Cash);
            var credentials = PluginContext.Operations.GetCredentials();
            PluginContext.Operations.AddExternalPaymentItem(order.ResultSum / 2, isProcessed, null, null, paymentType, order, credentials);
        }

        /// <summary>
        /// Добавление внешнего непроведенного платежа картой.
        /// </summary>
        private void AddCardExternalNotProcessedPayment()
        {
            const bool isProcessed = false;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            var credentials = PluginContext.Operations.GetCredentials();
            PluginContext.Operations.AddExternalPaymentItem(order.ResultSum / 2, isProcessed, additionalData, null, paymentType, order, credentials);
        }

        /// <summary>
        /// Добавление внешнего плагинного непроведенного платежа.
        /// </summary>
        private void AddPluginExternalNotProcessedPayment()
        {
            const bool isProcessed = false;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.External && x.Name == "SampleApiPayment");
            var additionalData = new ExternalPaymentItemAdditionalData { CustomData = Serializer.Serialize(new PaymentAdditionalData { SilentPay = true }) };
            var credentials = PluginContext.Operations.GetCredentials();
            PluginContext.Operations.AddExternalPaymentItem(order.ResultSum / 2, isProcessed, additionalData, null, paymentType, order, credentials);
        }

        /// <summary>
        /// Добавление внешнего проведенного платежа наличными.
        /// </summary>
        private void AddCashExternalProcessedPayment()
        {
            const bool isProcessed = true;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Cash);
            var credentials = PluginContext.Operations.GetCredentials();
            PluginContext.Operations.AddExternalPaymentItem(order.ResultSum / 2, isProcessed, null, null, paymentType, order, credentials);
        }

        /// <summary>
        /// Добавление внешнего проведенного платежа картой.
        /// </summary>
        private void AddCardExternalProcessedPayment()
        {
            const bool isProcessed = true;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            var credentials = PluginContext.Operations.GetCredentials();
            PluginContext.Operations.AddExternalPaymentItem(order.ResultSum / 2, isProcessed, additionalData, null, paymentType, order, credentials);
        }

        #endregion Add or delete external payment

        #region Add and process prepay

        /// <summary>
        /// Добавление внешнего проведенного платежа наличными с превращением его в предоплату в iiko.
        /// </summary>
        private void AddCashExternalProcessedPrepay()
        {
            const bool isProcessed = true;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Cash);
            var credentials = PluginContext.Operations.GetCredentials();
            var paymentItem = PluginContext.Operations.AddExternalPaymentItem(150, isProcessed, null, null, paymentType, order, credentials);
            order = PluginContext.Operations.GetOrderById(order.Id);

            PluginContext.Operations.ProcessPrepay(credentials, order, paymentItem);
        }

        /// <summary>
        /// Добавление внешнего проведенного платежа картой с превращением его в предоплату в iiko.
        /// </summary>
        private void AddCardExternalProcessedPrepay()
        {
            const bool isProcessed = true;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            var credentials = PluginContext.Operations.GetCredentials();
            var paymentItem = PluginContext.Operations.AddExternalPaymentItem(150, isProcessed, additionalData, null, paymentType, order, credentials);
            order = PluginContext.Operations.GetOrderById(order.Id);

            PluginContext.Operations.ProcessPrepay(credentials, order, paymentItem);
        }

        /// <summary>
        /// Добавление внешнего плагинного непроведенного платежа с превращением его в предоплату в iiko.
        /// </summary>
        private void AddPluginExternalNotProcessedPrepay()
        {
            const bool isProcessed = false;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.External && x.Name == "SampleApiPayment");
            var additionalData = new ExternalPaymentItemAdditionalData { CustomData = Serializer.Serialize(new PaymentAdditionalData { SilentPay = true }) };
            var credentials = PluginContext.Operations.GetCredentials();
            var paymentItem = PluginContext.Operations.AddExternalPaymentItem(150, isProcessed, additionalData, null, paymentType, order, credentials);

            PluginContext.Operations.ProcessPrepay(credentials, order, paymentItem);
        }

        #endregion Add and process prepay

        #region Add and process or delete donation

        private void AddCashProcessedDonation()
        {
            const bool isProcessed = true;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var donationType = PluginContext.Operations.GetDonationTypesCompatibleWith(order).First(dt => dt.PaymentTypes.Any(pt => pt.Kind == PaymentTypeKind.Cash));
            var paymentType = donationType.PaymentTypes.First(x => x.Kind == PaymentTypeKind.Cash);
            var credentials = PluginContext.Operations.GetCredentials();

            var paymentItem = PluginContext.Operations.AddDonation(credentials, order, donationType, paymentType, null, isProcessed, order.ResultSum / 10);
            order = PluginContext.Operations.GetOrderById(order.Id);
            Debug.Assert(order.Donations.Contains(paymentItem));
        }

        private void AddCardProcessedDonation()
        {
            const bool isProcessed = true;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.Closed);
            var donationType = PluginContext.Operations.GetDonationTypesCompatibleWith(order).First(dt => dt.PaymentTypes.Any(pt => pt.Kind == PaymentTypeKind.Card));
            var paymentType = donationType.PaymentTypes.First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            var credentials = PluginContext.Operations.GetCredentials();

            var paymentItem = PluginContext.Operations.AddDonation(credentials, order, donationType, paymentType, additionalData, isProcessed, order.ResultSum / 4);
            order = PluginContext.Operations.GetOrderById(order.Id);
            Debug.Assert(order.Donations.Contains(paymentItem));
        }

        private void AddPluginProcessedDonation()
        {
            const bool isProcessed = true;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var donationType = PluginContext.Operations.GetDonationTypesCompatibleWith(order).First(dt => dt.PaymentTypes.Any(pt => pt.Kind == PaymentTypeKind.External));
            var paymentType = donationType.PaymentTypes.First(x => x.Kind == PaymentTypeKind.External && x.Name == "SampleApiPayment");
            var credentials = PluginContext.Operations.GetCredentials();

            var paymentItem = PluginContext.Operations.AddDonation(credentials, order, donationType, paymentType, null, isProcessed, order.ResultSum / 3);
            order = PluginContext.Operations.GetOrderById(order.Id);
            Debug.Assert(order.Donations.Contains(paymentItem));
        }

        private void AddPluginNotProcessedDonation()
        {
            const bool isProcessed = false;
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var donationType = PluginContext.Operations.GetDonationTypesCompatibleWith(order).First(dt => dt.PaymentTypes.Any(pt => pt.Kind == PaymentTypeKind.External));
            var paymentType = donationType.PaymentTypes.First(x => x.Kind == PaymentTypeKind.External && x.Name == "SampleApiPayment");
            var additionalData = new ExternalPaymentItemAdditionalData { CustomData = Serializer.Serialize(new PaymentAdditionalData { SilentPay = true }) };
            var credentials = PluginContext.Operations.GetCredentials();

            var paymentItem = PluginContext.Operations.AddDonation(credentials, order, donationType, paymentType, additionalData, isProcessed, order.ResultSum / 2);
            order = PluginContext.Operations.GetOrderById(order.Id);
            Debug.Assert(order.Donations.Contains(paymentItem));
        }

        private void DeleteDonation()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill || o.Status == OrderStatus.Closed);
            var paymentItem = order.Donations.Last();
            PluginContext.Operations.DeleteDonation(PluginContext.Operations.GetCredentials(), order, paymentItem);
        }

        #endregion Add and process or delete donation

        #region Add and process or delete external fiscalized payment

        /// <summary>
        /// Добавление внешнего фискализованного платежа картой.
        /// </summary>
        private void AddExternalFiscalizedPayment()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            var credentials = PluginContext.Operations.GetCredentials();

            PluginContext.Operations.AddExternalFiscalizedPaymentItem(50, additionalData, paymentType, order, credentials);
        }

        /// <summary>
        /// Добавление внешнего фискализованного платежа картой с превращением его в предоплату в iiko.
        /// </summary>
        private void AddExternalFiscalizedPrepay()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            var additionalData = new CardPaymentItemAdditionalData { CardNumber = "123456" };
            var credentials = PluginContext.Operations.GetCredentials();
            var paymentItem = PluginContext.Operations.AddExternalFiscalizedPaymentItem(50, additionalData, paymentType, order, credentials);

            order = PluginContext.Operations.GetOrderById(order.Id);
            PluginContext.Operations.ProcessPrepay(credentials, order, paymentItem);
        }

        /// <summary>
        /// Удаление внешнего фискализованного платежа.
        /// </summary>
        private void DeleteExternalFiscalizedPayment()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
            var paymentItem = order.Payments.Last(i => i.IsFiscalizedExternally);

            PluginContext.Operations.DeleteExternalFiscalizedPaymentItem(paymentItem, order, PluginContext.Operations.GetCredentials());
        }

        #endregion Add and process or delete external fiscalized payment

        #region Pay order

        /// <summary>
        /// Дистанционная (до-)оплата заказа наличными. Наличные учитываются, как задолженность сотрудника.
        /// </summary>
        private void PayOrderOnCashAndPayOutOnUser()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentType = PluginContext.Operations.GetPaymentTypesToPayOutOnUser().First(x => x.Kind == PaymentTypeKind.Cash);
            // Payment is possible by user with opened personal session.
            var credentials = PluginContext.Operations.AuthenticateByPin("777");
            PluginContext.Operations.PayOrderAndPayOutOnUser(credentials, order, paymentType, order.ResultSum / 2);
        }

        /// <summary>
        /// Дистанционная (до-)оплата заказа картой.
        /// </summary>
        private void PayOrderOnCardAndPayOutOnUser()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentType = PluginContext.Operations.GetPaymentTypesToPayOutOnUser().First(x => x.Kind == PaymentTypeKind.Card && x.Name.ToUpper() == "VISA");
            // Payment is possible by user with opened personal session.
            var credentials = PluginContext.Operations.AuthenticateByPin("777");
            PluginContext.Operations.PayOrderAndPayOutOnUser(credentials, order, paymentType, order.ResultSum / 2);
        }

        /// <summary>
        /// Дистанционная оплата заказа существующими в заказе платежами.
        /// </summary>
        private void PayOrderWithExistingPayments()
        {
            var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            // Payment is possible only by user with opened personal session.
            var credentials = PluginContext.Operations.AuthenticateByPin("777");
            PluginContext.Operations.PayOrder(credentials, order);
        }

        #endregion Pay order

        private void AddAndProcessPreliminaryPayment()
        {
            // код имитирует сценарий DM
            var order = PluginContext.Operations.GetDeliveryOrders().Last(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);
            var paymentType = PluginContext.Operations.GetPaymentTypes().First(i => i.Kind == PaymentTypeKind.Cash);
            var credentials = PluginContext.Operations.GetCredentials();
            // в существующую доставку добавим предваритальный платеж 1000р, с этой суммы гостю нужна сдача
            var paymentItem = PluginContext.Operations.AddPreliminaryPaymentItem(1000m, null, paymentType, order, credentials);

            // курьер доставил заказ, пора оплачивать
            PluginContext.Operations.ExecuteContinuousOperation(
                operations =>
                {
                    // изменим сумму предварительного платежа, здесь можно рассчитать сдачу (Сдача = СуммаПредвПлатежа - СуммаЗаказа)
                    operations.ChangePaymentItemSum(order.ResultSum, null, null, paymentItem, PluginContext.Operations.GetDeliveryOrderById(order.Id), credentials);
                    // предварительный платеж превращаем в предоплату
                    operations.ProcessPrepay(credentials, PluginContext.Operations.GetDeliveryOrderById(order.Id), paymentItem);
                });
        }

        private void CreateOrderAndProcessPrepayments()
        {
            PluginContext.Operations.ExecuteContinuousOperation( // запускаем длительную непрерывную операцию, используя всегда доступный глобальный PluginContext.Operations
                operations => // внутри этой операции используем полученный на вход существующий только на время этого вызова экземпляр operations
                {
                    var credentials = operations.GetCredentials();

                    // глобальный PluginContext.Operations здесь тоже допустимо использовать, если с ним уже написаны какие-то кеши, вспомогательные методы
                    // для выполнения операций чтения, не обязательно весь код переводить на локальный operations, в этой части оба экземпляра сервиса работают идентично:
                    var paymentType = PluginContext.Operations.GetPaymentTypes().First(x => x.CanBeExternalProcessed);

                    // Разница в том, что, например, созданный через PluginContext.Operations заказ немедленно станет доступен для редактирования всем желающим
                    // - пользователь сможет вносить изменения в этот заказ;
                    // - другие плагины, если они следят за появлением заказов, смогут что-то в нём поменять;
                    // - фоновые встроенные обработчики смогут выполнить автопечать на кухню, автозавершение приготовления и прочие операции
                    // Иными словами, если мы собирались создать заказ и затем сделать с ним ещё что-то, нет гарантии, что мы сможем выполнить это подряд,
                    // заказ могут успеть «угнать» (мы получим исключение EntityAlreadyInUseException) и
                    // внести несовместимые с нашими планами изменения (мы получим EntityModifiedException).

                    // По возможности выполняем действия в рамках одной сессии редактирования — это будет атомарно, «всё или ничего».
                    // Однако, некоторые действия невозможно выполнить в рамках сессии редактирования (как правило, это необратимые действия),
                    // тогда цепочку таких действий можно выполнить подряд в рамках одной непрерывной операции (continuous operation).
                    // При этом все объекты, изменённые нами через operations, для всех остальных будут неявно заблокированы до конца этой операции
                    var editSession = operations.CreateEditSession();
                    var orderStub = editSession.CreateOrder(null);
                    editSession.AddOrderGuest("Herbert", orderStub);
                    var paymentItemStub = editSession.AddExternalPaymentItem(42m, true, null, null, paymentType, orderStub);
                    var submittedEntities = operations.SubmitChanges(credentials, editSession);

                    // Заказ уже создан, все его видят, но редактировать можем только мы                    
                    var order = submittedEntities.Get(orderStub);
                    var paymentItem = submittedEntities.Get(paymentItemStub);

                    // Важно не задерживаться в этом месте слишком долго, дабы позволить другим тоже вносить изменения
                    // Все необходимые данные желательно получить и подготовить заранее (особенно если для этого нужны сетевые запросы к внешним сервисам)
                    operations.ProcessPrepay(credentials, order, paymentItem);
                });
        }

        private void OpenCafeSession()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            PluginContext.Operations.OpenCafeSession(credentials);
        }

        private void CloseCafeSession()
        {
            var credentials = PluginContext.Operations.GetCredentials();
            PluginContext.Operations.CloseCafeSession(credentials);
        }

        public void Dispose()
        {
            window.Dispatcher.InvokeShutdown();
            window.Dispatcher.Thread.Join();
        }
    }

    [Serializable]
    public sealed class PaymentAdditionalData
    {
        public bool SilentPay { get; set; }
    }

    internal static class Serializer
    {
        internal static string Serialize<T>(T data) where T : class
        {
            using (var sw = new StringWriter())
            using (var writer = XmlWriter.Create(sw))
            {
                new XmlSerializer(typeof(T)).Serialize(writer, data);
                return sw.ToString();
            }
        }
    }
}
