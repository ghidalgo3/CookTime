# babe-algorithms
Materializing all hard decisions into code.
# Hot Reloading Everything
I have not figured out how to get `dotnet watch` to work properly with VSCode's launch configurations, instead if you want to get a hot-reload loop going then you should open 2 terminal windows and run the following:
```bash
# In the first one (do this in the csproj directory):
dotnet watch run
# in the second one (do this in the package.json directory):
npm run start
```
Remarks:
1. The .NET process prints its PID on startup. You should attach to this process to debug the C# code.
1. The NPM process can be debuged from an instance of Edge that has been launched through VSCode. I suppose that's because VSCode attaches to the Edge debugger and can map breakpoints to running JavaScript but honestly (shrug).
1. The `npm run start` call is what prints the server port, usually 44481 (but this is controlled in file`.env.development`)

# Blog
The blog is served as static files.
If you add or modify a blog post, you only need to regenerate the static files.
To do that, run the following in the `Blog` directory:

```
jekyll build -d ../wwwroot/Blog
```

That will dump new blog contents into the static files directory `wwwroot`.
Commit the changes after you generate the new blog post.