﻿// Copyright (c) 2012 Semyon Grigorev <rsdpisuy@gmail.com>
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

namespace Brahma.FSharp.OpenCL.AST

type PTypes<'lang> =
    | Int32
    | Float32
    | Int64
    | Float64
    | Void

[<AbstractClass>]
type Type<'lang>()=
    inherit Node<'lang>()
    abstract Size:int

type PrimitiveType<'lang>(pType:PTypes<'lang>) =
    inherit Type<'lang>()
    override this.Size = 32
    override this.Children = []
    member this.Type = pType

type RefType<'lang>(baseType:Type<'lang>) =
    inherit Type<'lang>()
    override this.Size = baseType.Size
    override this.Children = []
    member this.BaseType = baseType