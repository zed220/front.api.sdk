﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using Resto.Front.Api.SamplePlugin.Properties;
using Resto.Front.Api.Attributes;
using Resto.Front.Api.Attributes.JetBrains;

namespace Resto.Front.Api.SamplePlugin
{
    /// <summary>
    /// Тестовый плагин для демонстрации возможностей Api.
    /// Автоматически не публикуется, для использования скопировать Resto.Front.Api.SamplePlugin.dll в Resto.Front.Main\bin\Debug\Plugins\Resto.Front.Api.SamplePlugin\
    /// </summary>
    [UsedImplicitly]
    [PluginLicenseModuleId(21005108)]
    public sealed class SamplePlugin : IFrontPlugin
    {
        private readonly Stack<IDisposable> subscriptions = new Stack<IDisposable>();

        public SamplePlugin()
        {
            PluginContext.Log.Info("Initializing SamplePlugin");

            if (Settings.Default.ExtendBillCheque)
                subscriptions.Push(new BillChequeExtender());

            subscriptions.Push(new ButtonsTester());
            subscriptions.Push(new EditorTester());
            //ExternalOperationsTester.TestCalculator();
            //subscriptions.Push(new CookingPriority.CookingPriorityManager());
            //subscriptions.Push(new DiagnosticMessagesTester.MessagesTester());
            //subscriptions.Push(new Restaurant.RestaurantViewer());
            //subscriptions.Push(new Restaurant.MenuViewer());
            //subscriptions.Push(new DiagnosticMessagesTester.StopListChangeNotifier());
            //subscriptions.Push(new DiagnosticMessagesTester.FrontScreenReminder());
            //subscriptions.Push(new Restaurant.StreetViewer());
            //subscriptions.Push(new Restaurant.ReservesViewer());
            //subscriptions.Push(new Restaurant.ClientViewer());
            //subscriptions.Push(new DiagnosticMessagesTester.OrderItemReadyChangeNotifier());
            //subscriptions.Push(new LicensingTester());
            //subscriptions.Push(new Kitchen.KitchenLoadMonitoringViewer());
            //subscriptions.Push(new Restaurant.BanquetAndReserveTester());
            //subscriptions.Push(new PreliminaryOrders.OrdersEditor());
            //subscriptions.Push(new Restaurant.SchemaViewer());
            //subscriptions.Push(new OrderStatusChangeNotifier());
            //subscriptions.Push(ChequeTaskProcessor.Register());
            //subscriptions.Push(new NotificationHandlers.BeforeOrderBillHandler());
            //subscriptions.Push(new NotificationHandlers.NavigatingToPaymentScreenHandler());
            //PersonalSessionsTester.Test();
            //subscriptions.Push(Screens.OrderEditScreen.AddProductByBarcode());
            //subscriptions.Push(Screens.OrderEditScreen.AddDiscountByCard());
            // добавляйте сюда другие подписчики

            PluginContext.Log.Info("SamplePlugin started");
        }

        public void Dispose()
        {
            while (subscriptions.Any())
            {
                var subscription = subscriptions.Pop();
                try
                {
                    subscription.Dispose();
                }
                catch (RemotingException)
                {
                    // nothing to do with the lost connection
                }
            }

            PluginContext.Log.Info("SamplePlugin stopped");
        }
    }
}