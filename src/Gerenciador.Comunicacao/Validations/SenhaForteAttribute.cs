
using System.ComponentModel.DataAnnotations;

namespace Gerenciador.Comunicacao.Validations;

public class SenhaForteAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string senha)
            return false;

        if (string.IsNullOrWhiteSpace(senha))
            return false;

        // Verificações de força da senha
        var temMinuscula = senha.Any(char.IsLower);
        var temMaiuscula = senha.Any(char.IsUpper);
        var temDigito = senha.Any(char.IsDigit);
        var temSimbolo = senha.Any(c => "@$!%*?&".Contains(c));
        var tamanhoMinimo = senha.Length >= 8;

        return temMinuscula && temMaiuscula && temDigito && temSimbolo && tamanhoMinimo;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"A {name} deve conter pelo menos 8 caracteres, incluindo: 1 letra minúscula, 1 maiúscula, 1 número e 1 símbolo (@$!%*?&)";
    }
}
