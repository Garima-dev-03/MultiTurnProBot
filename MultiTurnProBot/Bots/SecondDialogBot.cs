using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTurnProBot.Bots
{
    public class SecondDialogBot: ComponentDialog
    {
        public SecondDialogBot()
            : base(nameof(SecondDialogBot))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                EmpIdStepAsync,
                MentorNameStepAsync,
                SumaryInfoAsync
              
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);


        }
        private static async Task<DialogTurnResult> EmpIdStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your Employee Id.") },
                cancellationToken);
        }

        private static async Task<DialogTurnResult> MentorNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your mentor name.") },
               cancellationToken);
        }
        private static async Task<DialogTurnResult> SumaryInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thanks for the additional information."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
   
}
