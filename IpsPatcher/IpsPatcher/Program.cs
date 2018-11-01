//
//   Copyright (C) 2018 Pharap (@Pharap)
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//

using System;
using System.IO;

namespace IpsPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: IpsPatcher {patch} {file}");
                return;
            }

            using (var inStream = File.Open(args[0], FileMode.Open))
            using (var outStream = File.Open(args[1], FileMode.Open))
                Patcher.Patch(inStream, outStream);
        }
    }
}
