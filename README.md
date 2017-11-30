# Hangfire.JobDomains

![MIT License](https://img.shields.io/badge/license-MIT-orange.svg)

Hangfire.JobDomains 是 Hangfire 扩展，增加预制任务功能，提供对继承指定接口的任务类的动态创建，在 Hangfire 的UI 面板中增加一主菜单用于对对预制任务管理等相关操作。
项目目前仍在不断完善中...

![management](management.PNG)

## Features
待完善 

## Setup

```c#
GlobalConfiguration.Configuration.UseDomains("任务包基路径");    
``` 

## Caution
 

## License

Copyright (c) 2017

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
