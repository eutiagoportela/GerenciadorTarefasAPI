namespace Gerenciador.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre um erro relacionado ao banco de dados
/// </summary>
public class DatabaseException : Exception
{
    /// <summary>
    /// Código de erro original do banco de dados, se disponível
    /// </summary>
    public string? SqlErrorCode { get; }

    /// <summary>
    /// Entidade relacionada ao erro, se disponível
    /// </summary>
    public string? EntityName { get; }

    /// <summary>
    /// Indica se o erro está relacionado a uma constraint de integridade
    /// </summary>
    public bool IsConstraintViolation { get; }

    public DatabaseException() : base("Ocorreu um erro ao acessar o banco de dados")
    {
    }

    public DatabaseException(string message) : base(message)
    {
    }

    public DatabaseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public DatabaseException(string message, string? sqlErrorCode, string? entityName = null, bool isConstraintViolation = false)
        : base(message)
    {
        SqlErrorCode = sqlErrorCode;
        EntityName = entityName;
        IsConstraintViolation = isConstraintViolation;
    }

    public DatabaseException(string message, Exception innerException, string? sqlErrorCode, string? entityName = null, bool isConstraintViolation = false)
        : base(message, innerException)
    {
        SqlErrorCode = sqlErrorCode;
        EntityName = entityName;
        IsConstraintViolation = isConstraintViolation;
    }

    /// <summary>
    /// Cria uma mensagem de erro amigável baseada no código de erro e entidade
    /// </summary>
    public string GetFriendlyMessage()
    {
        if (IsConstraintViolation && !string.IsNullOrEmpty(EntityName))
        {
            return $"Não foi possível realizar esta operação devido a uma restrição no banco de dados relacionada a {EntityName}.";
        }

        if (!string.IsNullOrEmpty(SqlErrorCode))
        {
            return SqlErrorCode switch
            {
                "23505" => "Já existe um registro com estes dados.",
                "23503" => "Não é possível realizar esta operação porque o registro está relacionado a outros dados.",
                "23502" => "Dados obrigatórios não foram fornecidos.",
                "42P01" => "Erro na estrutura do banco de dados: tabela não encontrada.",
                "42703" => "Erro na estrutura do banco de dados: coluna não encontrada.",
                "28P01" => "Falha na autenticação com o banco de dados. Verifique as credenciais.",
                "3D000" => "Banco de dados não existe.",
                "57P03" => "O banco de dados não está disponível no momento.",
                "08006" => "Falha na conexão com o banco de dados.",
                _ => "Ocorreu um erro no banco de dados."
            };
        }

        return Message;
    }
}