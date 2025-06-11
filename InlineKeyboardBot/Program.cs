using InlineKeyboardBot.Configuration;

var builder = WebApplication.CreateBuilder(args);

// 🚀 Bot service'larini qo'shish - bitta method bilan
builder.Services.AddInlineKeyboardBot(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🔧 Middleware'larni qo'shish
app.UseBotMiddleware(app.Environment);

// 🎯 Bot'ni initialize qilish (Database + Webhook)
await app.InitializeBotAsync();

app.Run();