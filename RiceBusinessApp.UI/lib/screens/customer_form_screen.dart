import 'package:flutter/material.dart';
import '../models/customer.dart';
import '../services/customer_service.dart';

class CustomerFormScreen extends StatefulWidget {
  // If customer is null, it's a CREATE operation.
  // If customer has data, it's an UPDATE operation.
  final Customer? customer;

  const CustomerFormScreen({Key? key, this.customer}) : super(key: key);

  @override
  _CustomerFormScreenState createState() => _CustomerFormScreenState();
}

class _CustomerFormScreenState extends State<CustomerFormScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _mobileController = TextEditingController();
  final _addressController = TextEditingController();
  final _openingBalanceController = TextEditingController(text: '0');

  final CustomerService _customerService = CustomerService();
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    // Pre-fill the form if we are updating an existing customer
    if (widget.customer != null) {
      _nameController.text = widget.customer!.name;
      _mobileController.text = widget.customer!.mobileNumber ?? '';
      _addressController.text = widget.customer!.address ?? '';
      _openingBalanceController.text = widget.customer!.openingBalance.toString();
    }
  }

  Future<void> _saveCustomer() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isLoading = true);

    try {
      final Map<String, dynamic> data = {
        'name': _nameController.text,
        'mobileNumber': _mobileController.text.isEmpty ? null : _mobileController.text,
        'address': _addressController.text.isEmpty ? null : _addressController.text,
        'openingBalance': double.parse(_openingBalanceController.text),
      };

      if (widget.customer == null) {
        // CREATE
        await _customerService.createCustomer(data);
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Customer Created')));
      } else {
        // UPDATE (Notice we only send the fields allowed in UpdateCustomerDto)
        final Map<String, dynamic> updateData = {
          'name': _nameController.text,
          'mobileNumber': _mobileController.text.isEmpty ? null : _mobileController.text,
          'address': _addressController.text.isEmpty ? null : _addressController.text,
        };
        await _customerService.updateCustomer(widget.customer!.id, updateData);
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Customer Updated')));
      }
      Navigator.pop(context, true); // Return true to refresh list
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
    } finally {
      setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final isEditing = widget.customer != null;

    return Scaffold(
      appBar: AppBar(
        title: Text(isEditing ? 'Update Customer' : 'Add Customer'),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Form(
          key: _formKey,
          child: ListView(
            children: [
              TextFormField(
                controller: _nameController,
                decoration: const InputDecoration(labelText: 'Customer Name'),
                validator: (val) => val == null || val.isEmpty ? 'Required' : null,
              ),
              TextFormField(
                controller: _mobileController,
                decoration: const InputDecoration(labelText: 'Mobile Number'),
                keyboardType: TextInputType.phone,
              ),
              TextFormField(
                controller: _addressController,
                decoration: const InputDecoration(labelText: 'Address'),
              ),
              // Opening Balance should only be editable during creation, per usual accounting practices, 
              // but we show it disabled during edit.
              TextFormField(
                controller: _openingBalanceController,
                decoration: const InputDecoration(labelText: 'Opening Balance'),
                keyboardType: TextInputType.number,
                enabled: !isEditing, 
              ),
              const SizedBox(height: 20),
              _isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : ElevatedButton(
                      onPressed: _saveCustomer,
                      child: Text(isEditing ? 'Update' : 'Save'),
                    )
            ],
          ),
        ),
      ),
    );
  }
}
