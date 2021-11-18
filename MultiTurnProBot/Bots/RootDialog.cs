using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace MultiTurnProBot.Bots
{
    public class RootDialog: ComponentDialog
    {
        
        private readonly UserState _userState;
        public RootDialog(UserState userState)
           : base(nameof(RootDialog))
        {
            _userState = userState;
            AddDialog(new UserProfile());
            AddDialog(new SecondDialogBot());

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginChoiceStepAsync,
                InitialStepAsync,
                FinalStepAsync,
            }));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt), InitialStepAsyncValidation));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private Task<bool> InitialStepAsyncValidation(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            string textChoice = (promptContext.Context.Activity.Text).ToLower();
            var list = new List<string> { "userprofile","seconddialog" };
            if(list.Contains(textChoice))
            {
                return Task.FromResult(true);

            }
            return Task.FromResult(false);


        }
        private async Task<DialogTurnResult> BeginChoiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
               new PromptOptions
               {

                   Prompt = MessageFactory.Text("Select one to proceed with!"),
                   Choices = ChoiceFactory.ToChoices(new List<string> { "UserProfile", "SecondDialog" }),
                   RetryPrompt = MessageFactory.Text("Please select the one from the options. ")



               }, cancellationToken);
               
            
        }
        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            List<string> list = new List<string>();
            stepContext.Values["beginChoice"] = ((FoundChoice)stepContext.Result).Value;

            var choice = (((FoundChoice)stepContext.Result).Value).ToLower();
            if (choice=="userprofile")
            {
                
                return await stepContext.BeginDialogAsync(nameof(UserProfile), null, cancellationToken);
            }

            else  
            {
                return await stepContext.BeginDialogAsync(nameof(SecondDialogBot), null, cancellationToken);

            }
           
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you for all the information!"),cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }


    }
}
