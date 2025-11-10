using App.View;
using Core;

var repository = new Repository();
var app = new ConsoleApp(repository);
new MainLoop(app).Run();

return 0;