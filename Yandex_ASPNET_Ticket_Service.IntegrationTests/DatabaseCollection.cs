using Xunit;

namespace Yandex_ASPNET_Ticket_Service.IntegrationTests;

/// <summary>
/// XUnit collection для интеграционных тестов, использующих общий контейнер PostgreSQL.
/// Обеспечивает, что DatabaseFixture инициализируется и освобождается только один раз
/// для всех тестов, помеченных этой коллекцией.
/// </summary>
[CollectionDefinition("DatabaseCollection", DisableParallelization = true)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // Определение коллекции. Реализация не требуется.
    // DisableParallelization = true гарантирует, что тесты в этой коллекции
    // не запускаются параллельно, что безопасно при использовании общего ресурса.
}