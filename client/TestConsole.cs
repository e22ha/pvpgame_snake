using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace console_snake
{
    // атрибут, указывающий на то, что это класс содержит тесты
    [TestFixture]
    class TestConsole
    {

        [TestCase]

        {
            var exception = Assert.Throws<ArgumentException>(() => );
            Assert.That(exception.Message, Is.EqualTo(""));
        }



    }
}
