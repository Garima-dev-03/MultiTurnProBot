using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTurnProBot.Bots
{
    public class SecondDialogBot: ComponentDialog
    {
    
        public SecondDialogBot()
            : base(nameof(SecondDialogBot))
        {
         
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
           {
                EmpIdStepAsync,
                MentorNameStepAsync,
               
            }));

           
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
       
    }
   
}
