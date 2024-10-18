using FluentValidation;

namespace BattleShip.Models
{
    public class UndoRequestValidator : AbstractValidator<UndoRequest>
    {
        public UndoRequestValidator()
        {
            RuleFor(x => x.gameId).GreaterThanOrEqualTo(0).WithMessage("L'ID de jeu doit être supérieur ou égal à 0.");
            RuleFor(x => x.moves).GreaterThanOrEqualTo(0).WithMessage("Le nombre de mouvements doit être supérieur ou égal à 0.");
        }
    }

    public class AttackRequestValidator : AbstractValidator<AttackRequest>
    {
        public AttackRequestValidator()
        {
            RuleFor(x => x.gameId).GreaterThanOrEqualTo(0).WithMessage("L'ID de jeu doit être supérieur à 0.");
            RuleFor(x => x.row).InclusiveBetween(0, 12).WithMessage("La ligne doit être entre 0 et 12.");
            RuleFor(x => x.column).InclusiveBetween(0, 12).WithMessage("La colonne doit être entre 0 et 12.");
        }
    }

    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.username).NotEqual("Bot").WithMessage("Le username ne doit pas etre égal à Bot.");
            RuleFor(x => x.username).NotEmpty().WithMessage("Le username ne doit pas etre vide.");
        }
    }

    public class RestartGameRequestValidator : AbstractValidator<RestartGameRequest>
    {
        public RestartGameRequestValidator()
        {
            RuleFor(x => x.gameId).GreaterThanOrEqualTo(0).WithMessage("L'ID de jeu doit être supérieur à 0.");
            RuleFor(x => x.difficulty).InclusiveBetween(1, 3).WithMessage("La difficulté doit être entre 1 et 3.");
            RuleFor(x => x.gridSize).InclusiveBetween(8, 12).WithMessage("La taille de la grille doit être comprise entre 8 et 12.");

            RuleForEach(x => x.playerOneBoatPositions)
                .Must(ValidateBoatPositions)
                .WithMessage("Les positions des bateaux doivent être valides.");
            RuleForEach(x => x.playerTwoBoatPositions)
                .Must(ValidateBoatPositions)
                .WithMessage("Les positions des bateaux doivent être valides.");
        }

        private bool ValidateBoatPositions(KeyValuePair<char, List<List<int>>> boatPosition)
        {
            foreach (var coordinates in boatPosition.Value)
            {
                if (coordinates.Count != 2 || coordinates[0] < 0 || coordinates[1] < 0 || coordinates[0] > 9 || coordinates[1] > 9)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
