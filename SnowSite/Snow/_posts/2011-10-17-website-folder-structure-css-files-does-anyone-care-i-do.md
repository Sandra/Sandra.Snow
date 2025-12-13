---
layout: post
title: Website Folder Structure? CSS Files? Does anyone care? I do...
---

When we build website's, more often than not we have: Separation of Concerns. Even if at most it's just basic 3-tier Architecture

What annoys me though is a lot of time no one ever thinks about separation of concerns when it comes to the folder structure of their website, and where things go.

## Folders
The one thing that annoys me above all else, is when 'layout images' (images for layout, design, styling etc) of the website, are shoved into the same folder that holds what I call "Content Images".

For example:

    root/ 
    root/css/ 
    root/images/

What I prefer to do is structure it so the css folder has it's own images folder, specifically for layout images.

    root/ 
    root/css/ 
    root/css/images/ 
    root/images

<!--excerpt-->

To take it a step further, I often have images, icons, and fonts.

    root/ 
    root/css/ 
    root/css/fonts/ 
    root/css/icons/ 
    root/css/images/ 
    root/images

This way I don't have to worry about finding specific icons in a folder full of images, I don't have to worry about sifting through images to find ones related to the layout/design, they are all neatly tucked away in their own specific areas.

When it comes to folder structure however, NuGet gets in the way.

It would be nice if it allowed you to specify where file types should go. At the moment 'css' files seem to go into 'Content'. JavaScript files go into 'Scripts'. But what I would rather have is all lower-case names, and 'css' rather than 'content'. Until then, I just download those sort of dependencies manually and create my own folder structure.

## CSS
The next thing that I find annoying is the God Style Sheet. It's the StyleSheet that people name:

    Styles.css

It's the only one that exists in the project, it's over 100k in size, no comments, no christmas trees. Just lines and lines of fail.

The approach I used to take (before finding .LESS) was creating multiple StyleSheets that were small and easy to maintain, sometimes I would end up with up to 12, possibly more depending on the size of the website.

Each StyleSheet would be responsible for a specific task, so I would have something like the following:

    /css/reset.css (this is just one of those many reset StyleSheets found on the net) 
    /css/layout.css 
    /css/main.css 
    /css/header.css 
    /css/footer.css 
    /css/main-navigation.css 
    /css/...

When sections of the website were broken down like this, it did require flicking between StyleSheets now-n-then, but they became much smaller and easier to maintain. I also became far-less at risk of Unwanted Side Effects.

Now with .LESS and LESS, it's easy to have only a few StyleSheets, since now you can tab within a specific region and continue to write styles that only apply to that region.

For example, given a Footer element. You could come up with something like:

    footer {
        border-top: 2px #182a33 solid;
        padding-top: 30px;
        nav {
            a {
                font-family: @footer-link-font;
                font-size: @footer-link-size;
                color: @footer-link-color;
                text-decoration: none;
                &:hover {
                    color: @footer-link-color-hover;
                    text-decoration: underline;
                }    
            }
            li > span {
                color: @footer-text-color;
                font-family: @footer-text-font;
                font-size: @footer-text-size;
            }
            h4 {
                color: @footer-heading-color;
                font-family: @footer-heading-font;
                font-size: @footer-heading-size;
                font-weight: bold;
                margin-bottom: 35px;
                padding-top: 17px;
            }
            > ul > li {
                width: 165px;
                float: left;
                display: block;
                line-height: 18px;
            }
        }
    }

This would in turn generate the following CSS for you.

    footer {
      border-top: 2px #182a33 solid;
      padding-top: 30px;
    }
    footer nav a {
      font-family: arial;
      font-size: 12px;
      color: #00adee;
      text-decoration: none;
    }
    footer nav a:hover {
      color: #00adee;
      text-decoration: underline;
    }
    footer nav li > span {
      color: #a8a8a8;
      font-family: arial;
      font-size: 12px;
    }
    footer nav h4 {
      color: white;
      font-family: arial;
      font-size: 16px;
      font-weight: bold;
      margin-bottom: 35px;
      padding-top: 17px;
    }
    footer nav > ul > li {
      width: 165px;
      float: left;
      display: block;
      line-height: 18px;
    }
    
I find it much easier to manage writing 'LESS' then traditional CSS.

## Conclusion
I didn't want to go into too much detail, my main point is I think people should put a little more thought into how they structure their folders, files, naming, etc when creating websites. We focus so much time on trying to main our applications and code maintainable, but somehow neglect the website itself, and after a while we just end up fighting with it.


