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
    public class UserProfile: ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfileClass> _userProfileAccessor;
        public UserProfile(UserState userState):base(nameof(UserProfileClass))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfileClass>("UserProfileClass");

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                TransportStepAsync,
                NameStepAsync,
                NameConfirmStepAsync,
                AgeStepAsync,
                LocationStepAsync,
                ContactInfoStepAsync,
                ConfirmStepAsync,
                SummaryStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));


            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);

        }
        private static async Task<DialogTurnResult> TransportStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {

                    Prompt = MessageFactory.Text("Please enter your mode of transport."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Car", "Bus", "Bicycle" }),

                }, cancellationToken);
        }
        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["transport"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") },
                cancellationToken);
        }
        private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Would you like to give your age?")
                }
                , cancellationToken);
        }

        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                // User said "yes" so we will be prompting for the age.
                // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter your age."),
                    RetryPrompt = MessageFactory.Text("The value entered must be greater than 0 and less than 150."),
                };

                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
            }
            else
            {
                // User said "no" so we will skip the next step. Give -1 as the age.
                return await stepContext.NextAsync(-1, cancellationToken);

            }
        }

        private static async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["age"] = (int)stepContext.Result;

            var msg = (int)stepContext.Values["age"] == -1 ? "No age given." : $"I have your age as {stepContext.Values["age"]}.";

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please Enter your location for the pickup."),

            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }
      
        private static async Task<DialogTurnResult> ContactInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["location"] = (string)stepContext.Result;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Your location for the pickup is{stepContext.Result}"), cancellationToken);

            var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("PLease enter your number!(Enter 10 digit number only!)"),


                };
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
       
        }

        private static async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            stepContext.Values["contact"] = (string)stepContext.Result;
          
          
            var promtOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Do you want to see your information")
            };
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), promtOptions, cancellationToken);
        }
        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            

            if ((bool)stepContext.Result)
            {
                var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfileClass(), cancellationToken);
                userProfile.Transport = (string)stepContext.Values["transport"];
                userProfile.Name =      (string)stepContext.Values["name"];
                userProfile.Age =        (int)stepContext.Values["age"];
                userProfile.Location =   (string)stepContext.Values["location"];
                userProfile.Contact =    (string)stepContext.Values["contact"];

                var msg = $"I have your mode of transport as {userProfile.Transport} and your name as {userProfile.Name}";

                if (userProfile.Age != -1)
                {
                    msg += $" and your age as {userProfile.Age}";
                }
                msg += ".";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

               
            }
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }


    }
}
