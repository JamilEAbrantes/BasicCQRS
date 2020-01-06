using System;
using System.Collections.Generic;
using System.Linq;

namespace BasicCQRS
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var userHandler = new UserHandler();
            var createUserCommand = new CreateUserCommand("", "jamilabrantes.dev@gmail.com");
            var commandMessage = (CommandMessage)userHandler.Handle(createUserCommand);

            Console.WriteLine($"{ commandMessage.GetMessages() }");
            Console.ReadKey();
        }
    }

    #region --> Command

    public class CreateUserCommand : ICommand
    {
        public CreateUserCommand(
            string name,
            string email)
        {
            Name = name;
            Email = email;
        }

        public string Name { get; set; }

        public string Email { get; set; }

        public ICommandMessage ValidationCommand(ICommandMessage commandMessage)
        {
            if (string.IsNullOrEmpty(Name))
                commandMessage.AddError("Enter the name");

            if (string.IsNullOrEmpty(Email))
                commandMessage.AddError("Enter the e -mail");

            return commandMessage;
        }
    }

    #endregion

    #region --> Command handler

    public class UserHandler :
        ICommandHandler<CreateUserCommand>
    {
        private readonly ICommandMessage _commandMessage;

        public UserHandler()
        {
            _commandMessage = new CommandMessage();
        }

        public ICommandMessage Handle(CreateUserCommand command)
        {
            var commandMessage = command.ValidationCommand(_commandMessage);
            if (commandMessage.InvalidCommand)
            {
                return commandMessage;
            }

            if (command.Name == "jamil")
            {
                return commandMessage.AddError("Customer already exists.");
            }

            return commandMessage.AddSuccess("Successfully registered customer.");
        }
    }

    #endregion

    #region --> Command contracts

    public interface ICommand
    {
        ICommandMessage ValidationCommand(ICommandMessage commandMessage);
    }

    public interface ICommandMessage
    {
        public bool InvalidCommand { get; set; }

        List<string> Errors { get; }

        List<string> Success { get; }

        ICommandMessage AddError(params string[] error);

        ICommandMessage AddSuccess(params string[] success);

        string GetMessages();
    }

    public class CommandMessage : ICommandMessage
    {
        public CommandMessage()
        {
            Errors = new List<string>();
            Success = new List<string>();
        }

        public List<string> Errors { get; }

        public List<string> Success { get; }

        public bool InvalidCommand { get; set; }

        public ICommandMessage AddError(params string[] error)
        {
            if (error != null)
            {
                Errors.AddRange(error);

                if (!InvalidCommand)
                {
                    InvalidCommand = true;
                }
            }

            return this;
        }

        public ICommandMessage AddSuccess(params string[] success)
        {
            if (success != null)
            {
                Success.AddRange(success);
            }

            return this;
        }

        #region --> Facilities

        public string GetMessages()
        {
            if (Errors.Any())
                return Errors.Select(item => string.Format("{0}", item)).Aggregate((texto, next) => texto + ", " + next);

            if (Success.Any())
                return Success.Select(item => string.Format("{0}", item)).Aggregate((texto, next) => texto + ", " + next);

            return string.Empty;
        }

        #endregion
    }

    public interface ICommandHandler<T> where T : ICommand
    {
        ICommandMessage Handle(T command);
    }

    #endregion    
}