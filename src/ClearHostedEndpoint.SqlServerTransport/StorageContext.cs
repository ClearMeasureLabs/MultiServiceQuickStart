using System.Data;
using System.Data.Common;
using Dapper;

namespace ClearMeasure.HostedEndpoint.SqlServerTransport;

/// <summary>
/// Provides a synchronized storage context for NServiceBus handlers to execute database operations
/// within the same transaction as message processing. This prevents escalation to distributed transactions.
/// </summary>
/// <remarks>
/// StorageContext should be injected as a transient dependency in message handlers.
/// The NServiceBus behavior (<see cref="StorageContextBehavior"/>) will attach the ambient
/// database connection and transaction from the synchronized storage session.
/// </remarks>
public class StorageContext : IDisposable
{
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    /// <summary>
    /// Gets the database connection. Throws if not attached by the behavior.
    /// </summary>
    public IDbConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                throw new InvalidOperationException(
                    "StorageContext.Connection is not available. Ensure StorageContextBehavior is registered and the message is being processed within a pipeline.");
            }
            return _connection;
        }
    }

    /// <summary>
    /// Gets the database transaction. Throws if not attached by the behavior.
    /// </summary>
    public IDbTransaction Transaction
    {
        get
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException(
                    "StorageContext.Transaction is not available. Ensure StorageContextBehavior is registered and the message is being processed within a pipeline.");
            }
            return _transaction;
        }
    }

    /// <summary>
    /// Attaches the database connection and transaction from the NServiceBus synchronized storage session.
    /// This method is called by <see cref="StorageContextBehavior"/>.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="transaction">The database transaction.</param>
    internal void Attach(IDbConnection connection, IDbTransaction transaction)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }

    /// <summary>
    /// Executes a query and returns the result set.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="sql">The SQL query.</param>
    /// <param name="param">The query parameters.</param>
    /// <param name="commandTimeout">The command timeout in seconds.</param>
    /// <param name="commandType">The command type.</param>
    /// <returns>A collection of results.</returns>
    public async Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object? param = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        return await Connection.QueryAsync<T>(
            sql,
            param,
            Transaction,
            commandTimeout,
            commandType);
    }

    /// <summary>
    /// Executes a single-row query.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="sql">The SQL query.</param>
    /// <param name="param">The query parameters.</param>
    /// <param name="commandTimeout">The command timeout in seconds.</param>
    /// <param name="commandType">The command type.</param>
    /// <returns>The first result or default.</returns>
    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? param = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        return await Connection.QuerySingleOrDefaultAsync<T>(
            sql,
            param,
            Transaction,
            commandTimeout,
            commandType);
    }

    /// <summary>
    /// Executes a single-row query.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="sql">The SQL query.</param>
    /// <param name="param">The query parameters.</param>
    /// <param name="commandTimeout">The command timeout in seconds.</param>
    /// <param name="commandType">The command type.</param>
    /// <returns>The first result.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no result is found.</exception>
    public async Task<T> QuerySingleAsync<T>(
        string sql,
        object? param = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        return await Connection.QuerySingleAsync<T>(
            sql,
            param,
            Transaction,
            commandTimeout,
            commandType);
    }

    /// <summary>
    /// Executes a query and returns the first result or default.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="sql">The SQL query.</param>
    /// <param name="param">The query parameters.</param>
    /// <param name="commandTimeout">The command timeout in seconds.</param>
    /// <param name="commandType">The command type.</param>
    /// <returns>The first result or default.</returns>
    public async Task<T?> QueryFirstOrDefaultAsync<T>(
        string sql,
        object? param = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        return await Connection.QueryFirstOrDefaultAsync<T>(
            sql,
            param,
            Transaction,
            commandTimeout,
            commandType);
    }

    /// <summary>
    /// Executes a query and returns the first result.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="sql">The SQL query.</param>
    /// <param name="param">The query parameters.</param>
    /// <param name="commandTimeout">The command timeout in seconds.</param>
    /// <param name="commandType">The command type.</param>
    /// <returns>The first result.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no result is found.</exception>
    public async Task<T> QueryFirstAsync<T>(
        string sql,
        object? param = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        return await Connection.QueryFirstAsync<T>(
            sql,
            param,
            Transaction,
            commandTimeout,
            commandType);
    }

    /// <summary>
    /// Executes a command that returns no results.
    /// </summary>
    /// <param name="sql">The SQL command.</param>
    /// <param name="param">The command parameters.</param>
    /// <param name="commandTimeout">The command timeout in seconds.</param>
    /// <param name="commandType">The command type.</param>
    /// <returns>The number of rows affected.</returns>
    public async Task<int> ExecuteAsync(
        string sql,
        object? param = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        return await Connection.ExecuteAsync(
            sql,
            param,
            Transaction,
            commandTimeout,
            commandType);
    }

    /// <summary>
    /// Executes a command and returns a scalar value.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="sql">The SQL command.</param>
    /// <param name="param">The command parameters.</param>
    /// <param name="commandTimeout">The command timeout in seconds.</param>
    /// <param name="commandType">The command type.</param>
    /// <returns>The scalar result.</returns>
    public async Task<T?> ExecuteScalarAsync<T>(
        string sql,
        object? param = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        return await Connection.ExecuteScalarAsync<T>(
            sql,
            param,
            Transaction,
            commandTimeout,
            commandType);
    }

    /// <summary>
    /// Executes a query and returns multiple result sets.
    /// </summary>
    /// <param name="sql">The SQL query.</param>
    /// <param name="param">The query parameters.</param>
    /// <param name="commandTimeout">The command timeout in seconds.</param>
    /// <param name="commandType">The command type.</param>
    /// <returns>A grid reader for multiple result sets.</returns>
    public async Task<SqlMapper.GridReader> QueryMultipleAsync(
        string sql,
        object? param = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        return await Connection.QueryMultipleAsync(
            sql,
            param,
            Transaction,
            commandTimeout,
            commandType);
    }

    /// <summary>
    /// Disposes of the storage context. Note: Connection and Transaction are managed by NServiceBus
    /// and should not be disposed here.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Do not dispose connection or transaction - they are managed by NServiceBus
            _connection = null;
            _transaction = null;
        }

        _disposed = true;
    }
}
