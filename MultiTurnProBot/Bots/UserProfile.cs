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
                    
                    Prompt = MessageFactory.Text("By what you travel to office. (Mode of transport)"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Four wheeler", "three wheeler", "Two wheeler" }),

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
            var name = stepContext.Result.ToString();

            stepContext.Values["name"] = (string)stepContext.Result;

        
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
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter your age."),
                    RetryPrompt = MessageFactory.Text("The value entered must be greater than 18 and less than 45."),
                };

                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
            }
            else
            {
               
                return await stepContext.NextAsync(-1, cancellationToken);

            }
        }

        private static async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var Age = (int)stepContext.Result;
            if(Age >= 18 && Age <= 45)
            {
                stepContext.Values["age"] = Age;
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I have your age as{stepContext.Values["age"] }"),cancellationToken);
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please Enter your location for the pickup."),

                };

                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }
           
            else
            {
                if(Age==-1)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("No age is given"), cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Invalid input"), cancellationToken);
                }
                stepContext.Values["age"] = 0;
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please Enter your location for the pickup."),

                };

                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }
            

        }
      
        private static async Task<DialogTurnResult> ContactInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["location"] = (string)stepContext.Result;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Your location for the pickup is {stepContext.Result}"), cancellationToken);

            var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("PLease enter your number!(Enter 10 digit number only!)"),


                };
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
       
        }

        private static async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var contact = (string)stepContext.Result;
            if(contact.Length==10 &&(contact.Substring(0, 1) == "9" || contact.Substring(0, 1) == "7" || contact.Substring(0, 1) == "8" || contact.Substring(0, 1) == "6"))
            {
                stepContext.Values["contact"] = contact;
                var promtOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Do you want to see your information")
                };
            
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), promtOptions, cancellationToken);

            }
            else
            {
                stepContext.Values["contact"] = "NIL";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Invalid number"));
                var promtOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Do you want to see your information")
                };
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), promtOptions, cancellationToken);

            }
          
         
           
        }
        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        { 

            if ((bool)stepContext.Result)
            {
                var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfileClass(), cancellationToken);
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
