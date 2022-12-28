using Confluent.Kafka;
using Moto.Events;
using Moto.Models;
using Newtonsoft.Json;

namespace Moto.Consumers
{
    public class UpdateProductQuantityConsumer : BackgroundService
    {
        private readonly string _topic;

        private readonly ConsumerConfig _config;

        private Action<string> _action;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        public UpdateProductQuantityConsumer(IServiceScopeFactory serviceScopeFactory)
        {
            _topic = "update-product-1";
            _action = UpdateProductQuantity;
            _config = new ConsumerConfig
            {
                GroupId = "st_consumer_group",
                BootstrapServers = "pkc-lzvrd.us-west4.gcp.confluent.cloud:9092",
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = "37TGLFSWA3CNWHX3",
                SaslPassword = "wZwiHlWhdC8+cHoKNw2U/qM1tznx0NpdfH0XLcQo+Ss2pOpnBaBfA3UH6sx0ZD1/",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _serviceScopeFactory = serviceScopeFactory;
        }

        void UpdateProductQuantity(string message)
        {
            var updateProducts = JsonConvert.DeserializeObject<List<UpdateProductQuantityEnvent>>(message);

            if (updateProducts == null || updateProducts.Count == 0) return;

            var updateIds = updateProducts.Select(p => p.ProductId).ToList();
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<MotoDBContext>();
                    var oldProducts = _context.Products
                                             .Where(p => updateIds.Contains(p.Id))
                                             .ToList();
                    if (updateProducts.Count != oldProducts.Count) return;

                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (var oldProduct in oldProducts)
                            {
                                var updateProduct = updateProducts.First(up => up.ProductId == oldProduct.Id);
                                if (updateProduct.Quantity > 0 && updateProduct.Quantity > oldProduct.Quantity)
                                    throw new Exception();

                                oldProduct.Quantity -= updateProduct.Quantity;
                            }

                            _context.SaveChanges();
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                        }

                    }
                }
            }
            catch (Exception ex)
            {

            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Update product consumer connecting to kafka server....");
            await Task.Factory.StartNew(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using (var builder = new ConsumerBuilder<Ignore, string>(_config).Build())
                    {
                        builder.Subscribe(_topic);
                        var cancelToken = new CancellationTokenSource();
                        try
                        {
                            while (true)
                            {
                                var consumer = builder.Consume(cancelToken.Token);
                                if (_action != null) _action(consumer.Message.Value);

                            }
                        }
                        catch (Exception)
                        {
                            builder.Close();
                        }
                    }
                }
            }
            );
        }
    }
}
