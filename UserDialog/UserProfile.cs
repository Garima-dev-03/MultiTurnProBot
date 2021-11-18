using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTurnProBot.Bots
{
    public class UserProfile: ComponentDialog
    {
      
        private static readonly string UserName = "userName";
        private static readonly string UserConatct = "userContact";
        private static readonly string UserChoice = "userChoice";
        public UserProfile()
            :base(nameof(UserProfile))
        {

            // This array defines how the Waterfall will execute.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
               {
              
                TransportStepAsync,
                NameStepAsync,
                NameConfirmStepAsync,
                AgeStepAsync,
                LocationStepAsync,
                ContactInfoStepAsync,
                ConfirmStepAsync,
                SummaryStepAsync,
            }));

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
    
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new TextPrompt(UserName, UserNameValidation));
            AddDialog(new TextPrompt(UserConatct, UserContactValidation));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), AgePromptValidatorAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ChoicePrompt(UserChoice, UserTransportValidation));
            AddDialog(new ChoicePrompt(UserChoice, UserTransportValidation));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));


            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);

        }

       

        private static Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
           
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 18 && promptContext.Recognized.Value < 45);
        }
        private Task<bool> UserNameValidation(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            string name = promptContext.Recognized.Value;
            name.Any(c => char.IsDigit(c));
            //if (Regex.IsMatch(name, @"^\d+$"))
            if (name.Any(c => char.IsDigit(c)))
            {
                return Task.FromResult(false);

            }
            else
            {
                return Task.FromResult(true);
            }
                
        }
        private Task<bool> UserContactValidation(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
         
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value.Length==10 && (promptContext.Recognized.Value.Substring(0,1)=="9" || promptContext.Recognized.Value.Substring(0, 1) == "7"||promptContext.Recognized.Value.Substring(0, 1) == "8"|| promptContext.Recognized.Value.Substring(0, 1) == "6"));
        }
        private async Task<bool> UserTransportValidation(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {       
           
            string text = (promptContext.Context.Activity.Text).ToLower();
            var list = new string [] {"four wheeler", "three wheeler","two wheeler"};
              if(list.Contains(text))
                {
                    return (true);

               }
            else {
                if (promptContext.AttemptCount > 3)
                {

                    await promptContext.Context.SendActivityAsync(MessageFactory.Text("Reached maximum limit of attempts"), cancellationToken); ;
                    return false;
                }
                else
                    return false;
               
                
            }
            
        }
       

        private static async Task<DialogTurnResult> TransportStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           
            return await stepContext.PromptAsync(UserChoice,
                new PromptOptions
                {
                    
                    Prompt = MessageFactory.Text("By what you travel to office. (Mode of transport)"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Four wheeler", "three wheeler", "Two wheeler" }),
                    RetryPrompt= MessageFactory.Text("Please select the one from the options. "),
                    

                }, cancellationToken);
        }
        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["transport"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync(UserName, new PromptOptions { Prompt = MessageFactory.Text("Please enter your name."),RetryPrompt= MessageFactory.Text("Name should be in alphabets only!") },
                cancellationToken);
        }
        private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var name = stepContext.Result.ToString();

            stepContext.Values["name"] = (string)stepContext.Result;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Would you like to give your age?"),
                    
                }
                , cancellationToken);
        }

        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter your age."),
                    RetryPrompt = MessageFactory.Text("The value entered must be greater than 18 and less than 45."),
                };
                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
            }
            else
            {
               
                return await stepContext.NextAsync(0, cancellationToken);

            }
        }
        private static async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
                stepContext.Values["age"] = (int)stepContext.Result;
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I have your age as {stepContext.Values["age"] }"), cancellationToken);
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please Enter your location for the pickup."),

                }; return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
                

        }
      
        private static async Task<DialogTurnResult> ContactInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["location"] = (string)stepContext.Result;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Your location for the pickup is {stepContext.Result}"), cancellationToken);

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("PLease enter your number!"),
                RetryPrompt = MessageFactory.Text("Invalid number!")
            };
                return await stepContext.PromptAsync(UserConatct, promptOptions, cancellationToken);
       
        }

        private static async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
                stepContext.Values["contact"] = (string)stepContext.Result;
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I have your contact as {stepContext.Result}"), cancellationToken);
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
                UserProfileClass userProfile = new UserProfileClass();
               // var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfileClass(), cancellationToken);
                userProfile.Transport = (string)stepContext.Values["transport"];
                userProfile.Name =   (string)stepContext.Values["name"];
                userProfile.Age =    (int)stepContext.Values["age"];
                userProfile.Location =(string)stepContext.Values["location"];
                userProfile.Contact = (string)stepContext.Values["contact"];
               
                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(ShowDetails(userProfile)), cancellationToken);


            }
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        public Attachment ShowDetails(UserProfileClass userProfileClass)
        {
            
            AdaptiveCard card = new AdaptiveCard("1.2") 
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveTextBlock
                    {
                        Text = "User's Information",
                        Size = AdaptiveTextSize.Large,
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                        Weight = AdaptiveTextWeight.Bolder,

                    },
                    new AdaptiveColumnSet()
                    {
                       Columns=new List<AdaptiveColumn>()
                       {
                            new AdaptiveColumn()
                            {
                              Items = new List<AdaptiveElement>()
                              {
                                  new AdaptiveTextBlock()
                                  {
                                    Text="Transport"
                                  }
                              }

                            },

                            new AdaptiveColumn()
                            {
                                Items= new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                      Text=userProfileClass.Transport
                                    }
                                }
                            },

                       }
                    },
                    new AdaptiveColumnSet()
                    {
                       Columns=new List<AdaptiveColumn>()
                       {
                            new AdaptiveColumn()
                            {
                              Items = new List<AdaptiveElement>()
                              {
                                  new AdaptiveTextBlock()
                                  {
                                    Text="Name"
                                  }
                              }

                            },
                            
                            new AdaptiveColumn()
                            {
                                Items= new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                      Text=userProfileClass.Name
                                    }
                                }
                            },
                           
                       }
                    },
                    new AdaptiveColumnSet()
                    {
                       Columns=new List<AdaptiveColumn>()
                       {
                            new AdaptiveColumn()
                            {
                              Items = new List<AdaptiveElement>()
                              {
                                  new AdaptiveTextBlock()
                                  {
                                    Text="Age"
                                  }
                              }

                            },

                            new AdaptiveColumn()
                            {
                                Items= new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                         Text=userProfileClass.Age.ToString()

                                    }
                                }
                            },

                       }
                    },
                    new AdaptiveColumnSet()
                    {
                       Columns=new List<AdaptiveColumn>()
                       {
                            new AdaptiveColumn()
                            {
                              Items = new List<AdaptiveElement>()
                              {
                                  new AdaptiveTextBlock()
                                  {
                                    Text="Location"
                                  }
                              }

                            },

                            new AdaptiveColumn()
                            {
                                Items= new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                      Text=userProfileClass.Location
                                    }
                                }
                            },

                       }
                    },
                     new AdaptiveColumnSet()
                    {
                       Columns=new List<AdaptiveColumn>()
                       {
                            new AdaptiveColumn()
                            {
                              Items = new List<AdaptiveElement>()
                              {
                                  new AdaptiveTextBlock()
                                  {
                                    Text="contact"
                                  }
                              }

                            },

                            new AdaptiveColumn()
                            {
                                Items= new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                      Text=userProfileClass.Contact.ToString()
                                    }
                                }
                            },

                       }
                    },


                },
              

            };
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,

            };
            return attachment;
        }


    }
}
