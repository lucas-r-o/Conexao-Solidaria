namespace Models.Extensions
{
    public static class StatusExtensions
    {
        public static bool Ativo(this Status status) => status == Status.Ativo;
        public static bool Concluido(this Status status) => status == Status.Concluido;
        public static bool Cancelado(this Status status) => status == Status.Cancelado;
    }
}
