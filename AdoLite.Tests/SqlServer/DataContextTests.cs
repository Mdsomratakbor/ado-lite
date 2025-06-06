//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using AdoLite.Core.Interfaces;
//using Xunit;

//namespace AdoLite.Tests.SqlServer
//{
//    public class DataContextTests
//    {
//        [Fact]
//        public void CanRegisterPostgresContext()
//        {
//            var services = new ServiceCollection();
//            services.UsePostgreSQL("Host=localhost;", ServiceLifetime.Scoped);

//            var provider = services.BuildServiceProvider();
//            var context = provider.GetService<IDataContext>();

//            Assert.NotNull(context);
//        }
//    }
//}
