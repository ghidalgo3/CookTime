# babe-algorithms
Materializing all hard decisions into code.

# Blog
The blog is served as static files.
If you add or modify a blog post, you only need to regenerate the static files.
To do that, run the following in the `Blog` directory:

```
jekyll build -d ../wwwroot/Blog
```

That will dump new blog contents into the static files directory `wwwroot`.
Commit the changes after you generate the new blog post.