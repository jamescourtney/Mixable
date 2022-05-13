from defusedxml.ElementTree import parse
import generated
import unittest

class SimpleTest(unittest.TestCase):
    # Returns True or False. 
    def test(self):
        doc = parse('../TestSchemas/derived2.xml');
        c = generated.Configuration(doc.getroot())

        self.assertEqual(c.Mapping.A, 182)
        self.assertEqual(c.Mapping.B, 'foo')
        self.assertEqual(c.Mapping.C.C, 2.0)
  
if __name__ == '__main__':
    unittest.main()