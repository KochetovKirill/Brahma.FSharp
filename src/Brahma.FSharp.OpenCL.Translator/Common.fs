﻿// Copyright (c) 2012, 2013 Semyon Grigorev <rsdpisuy@gmail.com>
// All rights reserved.
// 
// The contents of this file are made available under the terms of the
// Eclipse Public License v1.0 (the "License") which accompanies this
// distribution, and is available at the following URL:
// http://www.opensource.org/licenses/eclipse-1.0.php
// 
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
// the specific language governing rights and limitations under the License.
// 
// By using this software in any fashion, you are agreeing to be bound by the
// terms of the License.

[<AutoOpen>]
module Brahma.FSharp.OpenCL.Translator.Common

open Microsoft.FSharp.Quotations
open System.Collections.Generic
open Brahma.FSharp.OpenCL.AST

type Flags () =
    member val enableAtomic = false with get, set
    member val enableFP64 = false with get, set

type TranslatorOption =
    | BoolAsBit

type TargetContext<'lang, 'vDecl>() =
    let mutable topLevelVarsDeclarations = new ResizeArray<'vDecl>()
    let varDecls = new ResizeArray<'vDecl>()    
    let mutable flags = new Flags()
    let mutable namer = new Namer()
    let mutable tn = 0
    //let userDefinedTypes = new ResizeArray<System.Type>()
    //let userDeefinedTypeOpenCLDeclarations  = new System.Collections.Generic.Dictionary<System.Type,_>()
    let mutable translatorOptions = new ResizeArray<TranslatorOption>()
    member val tupleDecls = new Dictionary<string, int>() 
    member val tupleList = new List<Struct<Lang>>()    
    member val UserDefinedTypes = new ResizeArray<System.Type>()
    member val InLocal = false with get, set
    member val UserDefinedTypesOpenCLDeclaration = new System.Collections.Generic.Dictionary<string,Brahma.FSharp.OpenCL.AST.Struct<'lang>>()
    member this.tupleNumber 
        with get() = tn
        and set tn2 = tn <- tn2
    member this.TopLevelVarsDeclarations
        with get() = topLevelVarsDeclarations
        and  set v = topLevelVarsDeclarations <- v
    member this.VarDecls
        with get() = varDecls
    member this.Flags
        with get() = flags
        and set v = flags <- v
    member this.TranslatorOptions with get() = translatorOptions
    member this.Namer
        with get() = namer
        and set v = namer <- v

type Method(var:Var, expr:Expr) = 
    let funVar = var
    let funExpr = expr

    member this.FunVar:Var =
        funVar
    member this.FunExpr =
        funExpr

